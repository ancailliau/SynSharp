using Spectre.Console;
using Spectre.Console.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        AnsiConsole.Markup("[underline red]Welcome to Synsharp Console[/]\n");
        var app = new CommandApp<StormCommand>();
        return app.Run(args);
    }
}