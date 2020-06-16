using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
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

            var version = webHost.Services.GetService<IProductVersion>();
            var applicationVersion = version.ToString();
            var logger = webHost.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Designer application started. Version {version}", applicationVersion);

            await webHost
                .RunMigrations(typeof(M001_Init), "plainstore")
                .RunMigrations(typeof(M201904221727_AddErrorsTable), "logs")
                .RunAsync();
            
            return 0;
        }

        private static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((host, loggerConfig) =>
                {
                    loggerConfig
                        .ConfigureSurveySolutionsLogging(host.HostingEnvironment.ContentRootPath, "designer");

                    if (!host.HostingEnvironment.IsDevelopment())
                    {
                        loggerConfig.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);
                    }
                    
                    if (host.HostingEnvironment.IsDevelopment() || InDocker)
                    {
                        // To debug logitems source add {SourceContext} to output template
                        // outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                        loggerConfig.WriteTo.Console();
                    }
                })
                .ConfigureAppConfiguration((hostingContext, c) =>
                {
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile("appsettings.cloud.ini", true, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile("appsettings.Production.ini", true);
                    c.AddCommandLine(args);

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        c.AddUserSecrets<Startup>();
                    }
                });
        }
    }
}
