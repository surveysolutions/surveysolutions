using System;
using System.IO;
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
        public static void Main(string[] args)
        {
            var appRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            Log.Logger = new LoggerConfiguration()
                .ConfigureSurveySolutionsLogging(appRoot, "designer")
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .CreateLogger();

            try
            {
                var webHost = CreateWebHostBuilder(args).Build();
                var version = webHost.Services.GetService<IProductVersion>();
                var applicationVersion = version.ToString();

                Log.Logger.Warning("Designer application started. Version {version}", applicationVersion);

                webHost
                    .RunMigrations(typeof(M001_Init), "plainstore")
                    .RunMigrations(typeof(M201904221727_AddErrorsTable), "logs")
                    .Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .ConfigureAppConfiguration((hostingContext, c) =>
                {
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile("appsettings.cloud.ini", true, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile("appsettings.Production.ini", true);
                    c.AddCommandLine(args);

                    if(hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        c.AddUserSecrets<Startup>();
                    }
                });
        }
    }
}
