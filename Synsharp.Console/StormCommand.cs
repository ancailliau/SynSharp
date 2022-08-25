using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Synsharp;

public class StormCommand : AsyncCommand<StormCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-u|--username")]
        [DefaultValue("root")]
        public string Username { get; init; }

        [CommandOption("-p|--password")]
        [DefaultValue("secret")]
        public string Password { get; init; }
        
        [CommandOption("--url")]
        [DefaultValue("https://localhost:8902")]
        public string Url { get; set; }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole();
        });
        
        var synapseClient = new SynapseClient(settings.Url, loggerFactory);
        await synapseClient.LoginAsync(settings.Username, settings.Password);

        string currentView = null;
        while (true)
        {
            try
            {
                var query = AnsiConsole.Ask<string>("$> ");
                if (query.Equals("exit") | query.Equals("quit"))
                {
                    return 0;
                }
                
                if (query.Equals("!view"))
                {
                    AnsiConsole.WriteLine($"CurrentView: {currentView ?? "default"}");
                    var views = await synapseClient.View.List();
                    
                    var selection = AnsiConsole.Prompt(
                        new SelectionPrompt<KeyValuePair<string,string>>()
                            .Title("Select a [green]current view[/]?")
                            .PageSize(10)
                            .AddChoices(views.Select(_ => new KeyValuePair<string,string>(_.Iden, _.Name)).ToArray())
                            .UseConverter(_ => _.Value)
                        );
                    currentView = selection.Key;
                    continue;
                }
                
                var results = await synapseClient.StormAsync<SynapseObject>(query, new ApiStormQueryOpts() {View = currentView}).ToListAsync();

                var init = synapseClient.Init;
                var fini = synapseClient.Fini;

                foreach (var result in results)
                {
                    AnsiConsole.WriteLine(result.ToString() ?? string.Empty);
                }

                var plural = fini.Count > 1 ? "s" : "";
                AnsiConsole.WriteLine($"complete. {fini.Count} node{plural} in {fini.Took} ms.");
            }
            catch (SynapseException e)
            {
                AnsiConsole.WriteException(e);
            }
            catch (SynapseError e)
            {
                AnsiConsole.WriteException(e);
            }
            finally
            {
                AnsiConsole.WriteLine();   
            }
        }
        return 0;
    }
}