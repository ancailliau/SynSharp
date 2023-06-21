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

public class StormCommand : AsyncCommand<StormCommand.Settings>
{
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
        
        AnsiConsole.Markup("[underline red]Welcome to Synsharp Console[/]\n");

        using var client = new TelepathClient(settings.Url, loggerFactory: loggerFactory);
        client.OnLinked += async (sender, eventArgs) =>
        {
            AnsiConsole.MarkupLine("[bold green]Client is connected.[/]");
        };
        
        await REPL(client, loggerFactory);
        return 0;
    }
    
    private static async Task REPL(TelepathClient client, ILoggerFactory loggerFactory)
    {
        Synsharp.Telepath.Helpers.SynapseView currentView = null;
        string query;
        while (true)
        {
            var proxy = await client.GetProxyAsync(TimeSpan.FromSeconds(10));
            
            var prompt = "storm> ";
            if (currentView != null)
                if (string.IsNullOrEmpty(currentView.Name))
                    prompt = $"storm([magenta2]{currentView.Iden}[/])> ";
                else
                    prompt = $"storm([magenta2]{currentView.Name}[/])> ";
            
            query = AnsiConsole.Ask<string>(prompt);
            if (!query.StartsWith("!"))
            {
                Stopwatch watch = null;
                watch = Stopwatch.StartNew();

                var opts = new StormOps() { Repr = true, View = currentView?.Iden };
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
                    } else if (command[1] == "fork")
                    {
                        var view = await viewHelper.Fork();
                        if (view != null)
                        {
                            currentView = view;
                            AnsiConsole.MarkupLine($"Now on {view.Iden} ({view.Name})");
                        }
                    } else if (command[1] == "checkout")
                    {
                        if (command.Length > 2)
                        {
                            var view = await viewHelper.GetAsync(command[2]);   
                            if (view != null)
                            {
                                currentView = view;
                                AnsiConsole.MarkupLine($"Now on {view.Iden} ({view.Name})");
                            }
                        }
                    } else if (command[1] == "delete")
                    {
                        if (command.Length > 2)
                        {
                            await viewHelper.Delete(command[2]);
                            AnsiConsole.MarkupLine($"Deleted view");   
                        }
                    } else if (command[1] == "merge")
                    {
                        if (currentView != null)
                        {
                            await viewHelper.Merge(currentView.Iden);
                            AnsiConsole.MarkupLine($"Merged current view");
                            currentView = null;
                        }
                    }

                } else if (query == "!test")
                {
                    var node = new SynapseNode()
                    {
                        Form = "inet:ipv4",
                        Valu = "8.8.8.8",
                        Props = new Dictionary<string, dynamic>()
                        {
                            { "asn", 15169 }
                        },
                        Tags = new Dictionary<string, long?[]>()
                        {
                            { "google", new long?[] { } }
                        }
                    };


                    var variables = new Dictionary<string, dynamic>();
                    var stormQuery = new StringBuilder();

                    stormQuery.Append("[");

                    stormQuery.Append($" {node.Form}=$valu ");

                    foreach (var prop in node.Props)
                    {
                        stormQuery.Append($" :{prop.Key}=${prop.Key} ");
                        variables.Add(prop.Key, prop.Value);
                    }

                    foreach (var prop in node.Tags)
                    {
                        stormQuery.Append($" +#{prop.Key} ");
                    }

                    stormQuery.Append("]");

                    variables.Add("valu", node.Valu);

                    var watch = Stopwatch.StartNew();
                    var opts = new StormOps() { Repr = true, Vars = variables, View = currentView?.Iden };
                    await foreach (var o in proxy.Storm(stormQuery.ToString(), opts))
                    {
                        Display(o, watch);
                    }
                } else if (query.StartsWith("!count"))
                {
                    var strings = query.Split(' ', 2);
                    if (strings.Length > 1)
                    {
                        var subquery = strings[1]; 
                        var opts = new StormOps() { Repr = true, View = currentView?.Iden };
                        var count = await proxy.Count(subquery, opts);
                        AnsiConsole.MarkupLine($"[green]{count} nodes[/]");
                    }

                } else if (query == "!quit")
                {
                    break;
                }
            }
        }
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
            if (!string.IsNullOrEmpty(node.Repr))
            {
                AnsiConsole.Write(new Markup($"[dodgerblue1 bold]{node.Form}={node.Repr}[/] ({node.Iden})\n"));
            }
            else
            {
                AnsiConsole.Write(new Markup($"[dodgerblue1 bold]{node.Form}={node.Valu}[/] ({node.Iden})\n"));
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
                        text = $"({string.Join(",", ((Object[])prop.Value).Select(_ => _.ToString()))})";
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
                new Markup($"[orange b]SynapseWarn[/] [orange]{Markup.Escape(warn.Message)}[/]\n"));
        }
        else
        {
            AnsiConsole.Write(
                new Markup(
                    $"[grey]The message type '{Markup.Escape(o.GetType().FullName.ToString())}' is not supported (yet)[/]\n"));
        }
    }
}