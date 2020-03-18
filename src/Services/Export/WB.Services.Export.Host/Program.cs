using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Masking.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using WB.Services.Export.Host.Infra;
using WB.Services.Infrastructure.Logging;

namespace WB.Services.Export.Host
{
    class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    Console.WriteLine(eventArgs.ExceptionObject.GetType().FullName);
                    Console.WriteLine(eventArgs.ExceptionObject.ToString());
                    Log.Logger.Fatal("Unhandled exception occur {exception}", new[] { eventArgs.ExceptionObject.ToString() });
                };

                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);

                Directory.SetCurrentDirectory(pathToContentRoot);

                if (args.All(a => a != "--ignore-pid"))
                {
                    new StartupBlocker().OpenPIDFile();
                }

                var host = CreateWebHostBuilder(args).UseWindowsService();

                if (WindowsServiceHelpers.IsWindowsService())
                {
                    host = host.UseContentRoot(pathToContentRoot);
                }

                await host.Build().RunAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
        }

        private static void ConfigureSerilog(LoggerConfiguration logConfig, IConfiguration configuration)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            var fileLog = Path.Combine(Directory.GetCurrentDirectory(), "..", "logs", "export-service.log");
            var verboseLog = Path.Combine(Directory.GetCurrentDirectory(), "..", "logs", "export-service-verbose-.log");
            var errorDetailedLog = Path.Combine(Directory.GetCurrentDirectory(), "..", "logs", "export-service-errors-.log");

            var connectionString = GetConnectionString(configuration);

            logConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("AppType", "ExportService")
                .Enrich.WithProperty("workerId", "root")
                .Enrich.WithProperty("Version", fvi.FileVersion)
                .Enrich.WithProperty("VersionInfo", fvi.ProductVersion)
                .Enrich.WithProperty("Host", Environment.MachineName)
                .WriteTo.Postgres(connectionString, LogEventLevel.Error)
                .WriteTo.File(Path.GetFullPath(verboseLog), LogEventLevel.Verbose,
                    retainedFileCountLimit: 3, rollingInterval: RollingInterval.Day)
                .WriteTo.File(Path.GetFullPath(errorDetailedLog),
                    LogEventLevel.Error,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{Properties:j}",
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)

                .WriteTo
                    .File(Path.GetFullPath(fileLog), LogEventLevel.Debug,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day);


            var hook = configuration.GetSection("Slack").GetValue<string>("Hook");
            if (!string.IsNullOrWhiteSpace(hook))
            {
                var workerId = configuration.GetSection("job")["workerId"];
                logConfig = logConfig.WriteTo.Slack(hook, LogEventLevel.Fatal, workerId ?? "console");
            }

            var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

            foreach (var assemblyMetadataAttribute in metadata)
            {
                logConfig.Enrich.WithProperty(assemblyMetadataAttribute.Key, assemblyMetadataAttribute.Value);
            }
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
            var webConfig = configuration["webConfigs"];

            // should we go to web config for connection string?
            if (webConfig != null)
            {
                return WebConfigReader.ReadConnectionStringFromWebConfig(webConfig);
            }

            return configuration.GetConnectionString("DefaultConnection");
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hosting, logging) =>
                {
                    var logConfig = new LoggerConfiguration();
                    logConfig.Enrich.WithProperty("Environment", hosting.HostingEnvironment.EnvironmentName);
                    logConfig.WriteTo.Console(LogEventLevel.Debug,
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {workerId} {tenantName} #{jobId} {Message:lj}{NewLine}{Exception}");
                    logConfig.Destructure.ByMaskingProperties("Password", "ArchivePassword");

                    ConfigureSerilog(logConfig, hosting.Configuration);

                    Log.Logger = logConfig.CreateLogger();

                    if (!hosting.HostingEnvironment.IsDevelopment())
                    {
                        logging.ClearProviders();
                    }
                })
                .ConfigureWebHostDefaults(web =>
                {
                    if (!args.Contains("--kestrel"))
                    {
                        web.UseHttpSys();
                    }

                    web.UseStartup<Startup>();
                    web.ConfigureAppConfiguration(c =>
                    {
                        c.AddIniFile("appsettings.ini", false, true);
                        c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                        c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);

                        c.AddJsonFile($"appsettings.Cloud.json", true);
                        c.AddIniFile($"appsettings.Cloud.ini", true);

                        c.AddJsonFile($"appsettings.Production.json", true);
                        c.AddIniFile($"appsettings.Production.ini", true);

                        c.AddCommandLine(args);
                    });

                    web.UseSerilog();
                });
        }
    }
}
