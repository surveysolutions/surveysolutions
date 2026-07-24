using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.AspNetCore;
using WB.Infrastructure.AspNetCore.DataProtection;
using WB.UI.Headquarters.Services.EmbeddedService;

namespace WB.UI.Headquarters
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            if (args.Length > 0)
            {
                if (args[0].Equals("manage", StringComparison.OrdinalIgnoreCase))
                {
                    return await new SupportTool.SupportTool(host).Run(args.Skip(1).ToArray());
                }

                if (IsHelpArgument(args[0]))
                {
                    // Normalize single-dash -help to double-dash --help for System.CommandLine compatibility
                    var firstArg = args[0].Equals("-help", StringComparison.OrdinalIgnoreCase) ? "--help" : args[0];
                    return await new SupportTool.SupportTool(host).Run(new[] { firstArg }.Concat(args.Skip(1)).ToArray());
                }
            }

            // Npgsql.Logging.NpgsqlLogManager.Provider = new Npgsql.Logging.ConsoleLoggingProvider(Npgsql.Logging.NpgsqlLogLevel.Debug);
            var version = host.Services.GetRequiredService<IProductVersion>();
            var applicationVersion = version.ToString();
            var logger = host.Services.GetRequiredService<ILogger>();
            logger.Warning("HQ application starting. Version {version}", applicationVersion);
            logger.Warning($"Environment. Processor count: {System.Environment.ProcessorCount}");
            logger.Warning($"Environment. OSVersion: {System.Environment.OSVersion}");
            host.EnablePostgresXmlRepositoryLogging();
            await host.RunAsync();
            return 0;
        }

        private static bool IsHelpArgument(string arg) =>
            arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
            arg.Equals("-help", StringComparison.OrdinalIgnoreCase) ||
            arg.Equals("-h", StringComparison.OrdinalIgnoreCase);

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSurveySolutionsLogging("Headquarters", (host, logger) =>
                {
                    logger.MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning);
                })
                .ConfigureSurveySolutionsAppConfiguration<Startup>("HQ_", args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<HostOptions>(option =>
                    {
                        option.ShutdownTimeout = System.TimeSpan.FromSeconds(30);
                    });
                })
                .ConfigureEmbeddedServices()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
