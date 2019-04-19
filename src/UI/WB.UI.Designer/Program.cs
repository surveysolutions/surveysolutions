using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Migrations.PlainStore;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var logsFileLocation = Path.Combine(appRoot, "..", "logs", "log.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logsFileLocation, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning)
                .CreateLogger();

            try
            {
                var webHost = CreateWebHostBuilder(args).Build();
                var version = webHost.Services.GetService<IProductVersion>();
                var applicationVersion = version.ToString();

                Log.Logger.Warning("Designer application started. Version {version}", applicationVersion);

                webHost
                    .RunMigrations(typeof(M001_Init), "plainstore")
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
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile($"appsettings.Production.ini", true);
                    c.AddCommandLine(args);

                    if(hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        c.AddUserSecrets<Startup>();
                    }
                });
        }
    }
}
