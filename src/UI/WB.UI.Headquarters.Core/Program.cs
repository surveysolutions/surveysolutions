using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public static async Task<int> Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            if (args.Length > 0 && args[0].Equals("manage", StringComparison.OrdinalIgnoreCase))
            {
                return await new SupportTool.SupportTool(host).Run(args.Skip(1).ToArray());
            }
            
            var version = host.Services.GetRequiredService<IProductVersion>();
            var applicationVersion = version.ToString();
            var logger = host.Services.GetRequiredService<ILogger>();
            logger.Warning("HQ application starting. Version {version}", applicationVersion);

            await host.RunAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((host, loggerConfig) =>
                {
                    var logsFileLocation = Path.Combine(host.HostingEnvironment.ContentRootPath, "..", "logs", "log.log");
                    var verboseLog = Path.Combine(host.HostingEnvironment.ContentRootPath, "..", "logs", "verbose.log");

                    loggerConfig
                        //.MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.File(logsFileLocation, rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: LogEventLevel.Warning)
                        .WriteTo.File(verboseLog, rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: LogEventLevel.Verbose, retainedFileCountLimit: 2);
                    if (host.HostingEnvironment.IsDevelopment())
                    {
                        // To debug logitems source add {SourceContext} to output template
                        // outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                        loggerConfig.WriteTo.Console();
                    }
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((hostingContext, c) =>
                {
                    c.Sources.Clear();
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile("appsettings.DEV_DEFAULTS.ini", true, true);
                    c.AddIniFile("appsettings.cloud.ini", true, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile("appsettings.Production.ini", true, true);
                    c.AddEnvironmentVariables("HQ_");
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
