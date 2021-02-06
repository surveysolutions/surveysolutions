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
            if (args.Length > 0 && args[0].Equals("manage", StringComparison.OrdinalIgnoreCase))
            {
                return await new SupportTool.SupportTool(host).Run(args.Skip(1).ToArray());
            }

            // Npgsql.Logging.NpgsqlLogManager.Provider = new Npgsql.Logging.ConsoleLoggingProvider(Npgsql.Logging.NpgsqlLogLevel.Debug);
            var version = host.Services.GetRequiredService<IProductVersion>();
            var applicationVersion = version.ToString();
            var logger = host.Services.GetRequiredService<ILogger>();
            logger.Warning("HQ application starting. Version {version}", applicationVersion);
            host.EnablePostgresXmlRepositoryLogging();
            await host.RunAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureSurveySolutionsLogging("Headquarters", (host, logger) =>
                {
                    logger.MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning);
                })
                .ConfigureSurveySolutionsAppConfiguration<Startup>("HQ_", args)
                .ConfigureEmbeddedServices()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
