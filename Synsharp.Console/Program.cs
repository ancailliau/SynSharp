using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Synsharp.Telepath;
using Synsharp.Telepath.Helpers;
using Synsharp.Telepath.Messages;

namespace Synsharp.Console;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<StormCommand>();
        app.Configure(config =>
        {
            config.PropagateExceptions();
        });
        
        try
        {
            return app.Run(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -99;
        }
    }
    
}