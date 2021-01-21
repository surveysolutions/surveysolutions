using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace WB.Infrastructure.AspNetCore
{
    public static class AspnetCoreExtensions
    {
        public static LoggerConfiguration ConfigureSurveySolutionsLogging(this LoggerConfiguration logger,
            HostBuilderContext host, string projectName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            
            var logFolderLocation = host.Configuration.LoggingConfig().LogsLocation
                ?? Path.Combine(host.HostingEnvironment.ContentRootPath, "..", "logs");
             
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var verboseLog = Path.Combine(logFolderLocation, $"{projectName.ToLower()}.verbose.json..log");
            var logsFileLocation = Path.Combine(logFolderLocation, $"{projectName.ToLower()}..log");

            return logger
                .Destructure.ToMaximumDepth(4)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Environment", host.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("Version", fvi.FileVersion)
                .Enrich.WithProperty("VersionInfo", fvi.ProductVersion)
                .Enrich.WithProperty("AppType", projectName)
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning)
                .MinimumLevel.Override("Anemonis.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("WB.UI.Designer.Code.Attributes.BasicAuthenticationHandler", LogEventLevel.Warning)
#if !DEBUG
                .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Warning)
#endif
                .MinimumLevel.Override("Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService", LogEventLevel.Error)
                .MinimumLevel.Override("WB.UI.Headquarters.Code.Authentication.TenantTokenAuthenticationHandler", LogEventLevel.Information)
                .WriteTo.File(logsFileLocation, rollingInterval: RollingInterval.Day, 
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo
                    .File(new RenderedCompactJsonFormatter(), Path.GetFullPath(verboseLog), LogEventLevel.Verbose,
                        retainedFileCountLimit: 3, rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(host.Configuration, "Logging")
                ;    
        }

        public static IHostBuilder ConfigureSurveySolutionsLogging(this IHostBuilder hostBuilder,
            string projectName,
            Action<HostBuilderContext, LoggerConfiguration>? loggerConfigurationOverride = null)
        {
            return hostBuilder.UseSerilog((host, loggerConfig) =>
            {
                loggerConfig
                    .ConfigureSurveySolutionsLogging(host, projectName);

                loggerConfigurationOverride?.Invoke(host, loggerConfig);
                var console = host.Configuration["console"];

                if (console == "false") return;

                if (host.HostingEnvironment.IsDevelopment() || InDocker || host.Configuration["console"] != null)
                {
                    // To debug logitems source add {SourceContext} to output template
                    // outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                    loggerConfig.WriteTo
                        .Console(LogEventLevel.Debug,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {AppType:w3} {Message:lj}{NewLine}{Exception}"
                    );
                }
            }, preserveStaticLogger: true);
        }
        
        private static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        public static IHostBuilder ConfigureSurveySolutionsAppConfiguration<TStartup>(
            this IHostBuilder hostBuilder, 
            string envPrefix, 
            string [] args,
            bool useWebDefaults = true,
            Action<HostBuilderContext, IConfigurationBuilder>? configure = null)
            where TStartup : class
        {
            hostBuilder
                .ConfigureAppConfiguration((hostingContext, c) =>
            {
                c.AddIniFile("appsettings.ini", false, true);
                c.AddIniFile("appsettings.DEV_DEFAULTS.ini", true, true);
                c.AddIniFile("appsettings.cloud.ini", true, true);
                c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                c.AddIniFile("appsettings.Production.ini", true, true);

                configure?.Invoke(hostingContext, c);

                c.AddEnvironmentVariables(envPrefix);
                c.AddCommandLine(args);
            });

            if (useWebDefaults)
            {
                hostBuilder
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        if (args.Contains("--kestrel"))
                        {
                            webBuilder.UseKestrel();
                        }

                        if (args.Contains("--httpsys"))
                        {
                            webBuilder.UseHttpSys();
                        }

                        webBuilder.UseStartup<TStartup>();
                    });
            }
            else
            {
                Log.Information("Skipping {method} call", "ConfigureWebHostDefaults");
            }

            return hostBuilder;
        }
    }
}
