using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Headquarters
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var logsFileLocation = Path.Combine(appRoot, "..", "logs", "log.log");
            var verboseLog = Path.Combine(appRoot, "..", "logs", "verbose.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logsFileLocation, rollingInterval: RollingInterval.Day, 
                    restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.File(verboseLog, rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Verbose, retainedFileCountLimit: 2)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                var version = host.Services.GetRequiredService<IProductVersion>();
                var applicationVersion = version.ToString();

                Log.Logger.Warning("HQ application starting. Version {version}", applicationVersion);

                host.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((hostingContext, c) =>
                {
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile("appsettings.DEV_DEFAULTS.ini", true, true);
                    c.AddIniFile("appsettings.cloud.ini", true, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile("appsettings.Production.ini", true);
                    c.AddCommandLine(args);

                    if(hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        c.AddUserSecrets<Startup>();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseHttpSys();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
