using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Synsharp;
using Synsharp.Telepath;
using Synsharp.Telepath.Helpers;
using Synsharp.Telepath.Messages;
using SynapseView = Synsharp.Telepath.Helpers.SynapseView;

public class StormCommand : AsyncCommand<StormCommand.Settings>
{
    private bool _cancelConfirmed;
    private TelepathClient _client;
    private static CancellationTokenSource _cancellationTokenSource;
    private static SynapseView? _currentView;

    public sealed class Settings : CommandSettings
    {
        [Description("The url to the cell, e.g. tcp://visi:secret@localhost:27492/")]
        [CommandArgument(0, "[Url]")]
        public string Url { get; set; }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Synsharp", LogLevel.Trace)
                .AddConsole();
        });

        _cancellationTokenSource = new CancellationTokenSource();

        _cancelConfirmed = false;
        Console.CancelKeyPress += new ConsoleCancelEventHandler(cancelHandler);
        
        AnsiConsole.Markup("[underline red]Welcome to Synsharp Console[/]\n");

        _client = new TelepathClient(settings.Url, loggerFactory: loggerFactory);
        _client.OnConnect += async (sender, eventArgs) =>
        {
            AnsiConsole.MarkupLine("[bold green]Client is connected.[/]");
        };
        
        _client.OnDisconnect += async (sender, eventArgs) =>
        {
            AnsiConsole.MarkupLine("[bold orange1]Client disconnected.[/]");
        };
        
        await REPL(_client, loggerFactory);
        
        AnsiConsole.MarkupLine("[green]Closing client...[/]");
        _client.Dispose();
        
        return 0;
    }

    private void cancelHandler(object? sender, ConsoleCancelEventArgs e)
    {
        if (_cancelConfirmed)
        {
            _client?.Dispose();
        }
        else
        {
            AnsiConsole.Markup("\n[grey]Hit <Ctrl-C>, use !quit to quit or hit <Ctrl-C> again[/]\n");
            _cancelConfirmed = true;
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
        }
    }

    private static async Task REPL(TelepathClient client, ILoggerFactory loggerFactory)
    {
        _currentView = null;
        string query;
        while (true)
        {
            try
            {
                if (await QueryUser(client, loggerFactory)) break;
            }
            catch (TaskCanceledException ex)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (SynsharpError ex)
            {   
                AnsiConsole.MarkupLine($"[bold orange1]Something went wrong ({ex.Message}).[/]");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }
        }
    }

    private static async Task<bool> QueryUser(TelepathClient client, ILoggerFactory loggerFactory)
    {
        string query;
        var proxy = await client.GetProxyAsync(TimeSpan.FromSeconds(10));

        var prompt = "storm> ";
        if (_currentView != null)
            if (string.IsNullOrEmpty(_currentView.Name))
                prompt = $"storm([magenta2]{_currentView.Iden}[/])> ";
            else
                prompt = $"storm([magenta2]{_currentView.Name}[/])> ";

        var textPrompt = new TextPrompt<string>(prompt);

        query = await textPrompt.ShowAsync(AnsiConsole.Console, _cancellationTokenSource.Token);

        //query = AnsiConsole.Ask<string>(prompt);
        if (!query.StartsWith("!"))
        {
            Stopwatch watch = null;
            watch = Stopwatch.StartNew();

            var opts = new StormOps() { Repr = true, View = _currentView?.Iden };
            await foreach (var o in proxy.Storm(query, opts))
            {
                Display(o, watch);
            }

            AnsiConsole.MarkupLine("[b]Done[/]");
        }
        else
        {
            if (query.StartsWith("!view"))
            {
                var viewHelper = new ViewHelper(client, loggerFactory.CreateLogger<ViewHelper>());
                var command = query.Split(' ');
                if (command[1] == "list")
                {
                    var views = await viewHelper.List();
                    foreach (var view in views)
                    {
                        AnsiConsole.MarkupLine($"{view.Iden} ({view.Name})");
                    }
                }
                else if (command[1] == "fork")
                {
                    var view = await viewHelper.Fork();
                    if (view != null)
                    {
                        _currentView = view;
                        AnsiConsole.MarkupLine($"Now on {view.Iden} ({view.Name})");
                    }
                }
                else if (command[1] == "checkout")
                {
                    if (command.Length > 2)
                    {
                        var view = await viewHelper.GetAsync(command[2]);
                        if (view != null)
                        {
                            _currentView = view;
                            AnsiConsole.MarkupLine($"Now on {view.Iden} ({view.Name})");
                        }
                    }
                }
                else if (command[1] == "delete")
                {
                    if (command.Length > 2)
                    {
                        await viewHelper.Delete(command[2]);
                        AnsiConsole.MarkupLine($"Deleted view");
                    }
                }
                else if (command[1] == "merge")
                {
                    if (_currentView != null)
                    {
                        await viewHelper.Merge(_currentView.Iden);
                        AnsiConsole.MarkupLine($"Merged current view");
                        _currentView = null;
                    }
                }
            }
            else if (query == "!test")
            {
                AnsiConsole.MarkupLine($"[grey]Reserved command, do nothing.[/]");
            }
            else if (query.StartsWith("!count"))
            {
                var strings = query.Split(' ', 2);
                if (strings.Length > 1)
                {
                    var subquery = strings[1];
                    var opts = new StormOps() { Repr = true, View = _currentView?.Iden };
                    var count = await proxy.Count(subquery, opts);
                    AnsiConsole.MarkupLine($"[green]{count} nodes[/]");
                }
            }
            else if (query == "!quit")
            {
                return true;
            }
        }

        return false;
    }

    private static void Display(SynapseMessage o, Stopwatch? watch)
    {
        if (o is SynapseInit init)
        {
            AnsiConsole.Write(new Markup($"Started query [bold yellow]{Markup.Escape(init.Text)}[/]\n"));
        }
        else if (o is SynapseFini fini)
        {
            if (watch != null) watch.Stop();

            AnsiConsole.Write(new Markup(
                $"Query took [bold yellow]{fini.Took} ms[/] ({watch?.ElapsedMilliseconds.ToString() ?? "-"} ms) and returned [bold yellow]{fini.Count} node(s)[/]\n"));
        }
        else if (o is SynapseNode node)
        {
            if (node.Repr != null)
            {
              if (node.Repr is object[] arr) {
                AnsiConsole.Write(
                    new Markup($"[dodgerblue1 bold]{Markup.Escape(node.Form)}=({string.Join(", ", arr.Select(item => Markup.Escape(item.ToString())))})[/] ({node.Iden})\n"));
              } else if (node.Repr is object o2) {
                AnsiConsole.Write(new Markup($"[dodgerblue1 bold]{Markup.Escape(node.Form)}={Markup.Escape((string)node.Repr)}[/] ({Markup.Escape(o2.ToString())})\n"));
                
              }
            }
            else
            {
                if (node.Valu is object[] arr)
                {
                    var str = string.Join(", ", arr.Select(_ => Markup.Escape(_.ToString())));
                    AnsiConsole.Write(new Markup($"[dodgerblue1 bold]{Markup.Escape(node.Form)}=({str})[/] ({node.Iden})\n"));
                } else if (node.Valu is object o2)
                {
                    AnsiConsole.Write(new Markup($"[dodgerblue1 bold]{Markup.Escape(node.Form)}={Markup.Escape(node.Valu)}[/] ({node.Iden})\n"));
                }
            }

            foreach (var prop in node.Props)
            {
                if (node.Reprs != null && node.Reprs.ContainsKey(prop.Key))
                {
                    var text = "";
                    if (node.Reprs[prop.Key] is object[] array)
                    {
                        text = $"({string.Join(",", array.Select(_ => _.ToString()))})";
                    }
                    else
                    {
                        text = node.Reprs[prop.Key]?.ToString();   
                    }
                    AnsiConsole.Write(new Markup(
                        $"\t[dodgerblue1]{Markup.Escape(prop.Key?.ToString())}={Markup.Escape(text)}[/]\n"));
                }
                else
                {
                    var text = "";
                    if (prop.Value.GetType() == typeof(Object[]))
                    {
                        text = $"({string.Join(",", ((Object[])prop.Value).Select(_ => Markup.Escape(_.ToString())))})";
                    }
                    else
                    {
                        text = prop.Value?.ToString();   
                    }
                    AnsiConsole.Write(new Markup(
                        $"\t[dodgerblue1]{Markup.Escape(prop.Key?.ToString())}={Markup.Escape(text)}[/]\n"));
                }
            }

            foreach (var prop in node.Tags)
            {
                var text = prop.Key?.ToString();
                var dates = prop.Value;
                AnsiConsole.Write(new Markup(
                    $"\t[dodgerblue1]#{Markup.Escape(text)} ({Markup.Escape(dates[0].ToString() ?? "-")},{Markup.Escape(dates[0].ToString() ?? "-")})[/]\n"));
            }
        }
        else if (o is SynapsePrint print)
        {
            AnsiConsole.Write(new Markup($"[dodgerblue1]{Markup.Escape(print.Message)}[/]\n"));
        }
        else if (o is SynapseErr err)
        {
            AnsiConsole.Write(
                new Markup($"[red b]SynapseErr[/] [red]{Markup.Escape(err.ErrorType ?? "")}: {Markup.Escape(err.Message ?? "")}[/]\n"));
        }
        else if (o is SynapseWarn warn)
        {
            AnsiConsole.Write(
                new Markup($"[red b]SynapseWarn[/] [red]{Markup.Escape(warn.Message)}[/]\n"));
        }
        else
        {
            AnsiConsole.Write(
                new Markup(
                    $"[grey]The message type '{Markup.Escape(o.GetType().FullName.ToString())}' is not supported (yet)[/]\n"));
        }
    }
}