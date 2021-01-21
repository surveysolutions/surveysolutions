using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.AspNetCore;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Migrations.Logs;
using WB.UI.Designer.Migrations.PlainStore;

namespace WB.UI.Designer
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            if (args.Length > 0 && args[0].Equals("manage", StringComparison.OrdinalIgnoreCase))
            {
                return await new SupportTool.SupportTool(webHost).Run(args.Skip(1).ToArray());
            }

            var version = webHost.Services.GetRequiredService<IProductVersion>();
            var applicationVersion = version.ToString();
            var logger = webHost.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Designer application started. Version {version}", applicationVersion);

            await webHost
                .RunMigrations(typeof(M001_Init), "plainstore")
                .RunMigrations(typeof(M201904221727_AddErrorsTable), "logs")
                .RunAsync();
            
            return 0;
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureSurveySolutionsLogging("designer",(host, loggerConfig) =>
                {
                    if (!host.HostingEnvironment.IsDevelopment())
                    {
                        loggerConfig.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);
                    }
                })
                .ConfigureSurveySolutionsAppConfiguration<Startup>("DESIGNER_", args);
        }
    }
}
