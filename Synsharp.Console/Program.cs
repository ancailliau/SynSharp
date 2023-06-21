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
        return app.Run(args);
    }
    
}