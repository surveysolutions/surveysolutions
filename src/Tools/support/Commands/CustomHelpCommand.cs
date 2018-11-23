using System;
using System.Threading.Tasks;
using NConsole;

namespace support
{
    internal class CustomHelpCommand : IConsoleCommand
    {
        public Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            host.WriteLine("usage: support <command> [<args>]");
            host.WriteLine("These are common Support commands used in various situations:");
            host.WriteLine();
            host.WriteHighlightedText("Health check of Survey Solutions services", ConsoleColor.DarkGreen);
            host.WriteLine();
            host.WriteLine("usage: support healh-check /path:\"<path>\"");
            host.WriteLine("                    [--all] [--survey-solutions | -ss]");
            host.WriteLine("                    [--database-connection | -dbc]");
            host.WriteLine("                    [--database-permissions | -dbp]");
            host.WriteLine("Options:");
            host.WriteLine();
            host.WriteLine("<path>");
            host.WriteLine("   Physical path to Headquarters website.");
            host.WriteLine("   If you are using SurveySolutions installer, by default path to Headquarters website is: C:\\\\Site");
            host.WriteLine();
            host.WriteLine("--all");
            host.WriteLine("   Run all health checks. All health checks include: ");
            host.WriteLine("      * Check access to Survey Solutions website");
            host.WriteLine("      * Check access to Headquarters database");
            host.WriteLine("      * Check permissions to Headquarters database. Check that user which connected to database is owner of that database");
            host.WriteLine("-ss");
            host.WriteLine("--survey-solutions");
            host.WriteLine("   Check access to Survey Solutions website");
            host.WriteLine("-dbc");
            host.WriteLine("--database-connection");
            host.WriteLine("   Check access to Headquarters database");
            host.WriteLine("-dbp");
            host.WriteLine("--database-permissions");
            host.WriteLine("   Check permissions to Headquarters database. Check that user which connected to database is owner of that database");
            host.WriteLine();
            host.WriteHighlightedText("Archive Headquarters log files", ConsoleColor.DarkGreen);
            host.WriteLine();
            host.WriteLine("usage: support archive-logs /path:\"<path>\"");
            host.WriteLine();
            host.WriteLine("Options:");
            host.WriteLine();
            host.WriteLine("<path>");
            host.WriteLine("   Physical path to Headquarters website.");
            host.WriteLine("   If you are using SurveySolutions installer, by default path to Headquarters website is: C:\\\\Site");
            host.WriteLine();


            return Task.FromResult<object>(null);
        }
    }
}