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
using Serilog.Formatting.Json;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.AspNetCore;

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
        private static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((host, loggerConfig) =>
                {
                    loggerConfig
                        .ConfigureSurveySolutionsLogging(host.HostingEnvironment.ContentRootPath, "Headquarters")
                        .MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning);

                    if (host.HostingEnvironment.IsDevelopment() || InDocker)
                    {
                        // To debug logitems source add {SourceContext} to output template
                        // outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                        loggerConfig.WriteTo.Console();
                    }
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((hostingContext, c) =>
                {
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile("appsettings.DEV_DEFAULTS.ini", true, true);
                    c.AddIniFile("appsettings.cloud.ini", true, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile("appsettings.Production.ini", true, true);
                    c.AddEnvironmentVariables("HQ_");
                    c.AddCommandLine(args);

                    if (hostingContext.HostingEnvironment.IsDevelopment())
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
