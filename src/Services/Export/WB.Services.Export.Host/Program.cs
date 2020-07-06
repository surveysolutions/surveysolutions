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
using WB.Infrastructure.AspNetCore;
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

                if (args.All(a => a != "--ignore-pid"))
                {
                    new StartupBlocker().OpenPIDFile();
                }

                var host = CreateWebHostBuilder(args).UseWindowsService();

                if (WindowsServiceHelpers.IsWindowsService())
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    var pathToContentRoot = Path.GetDirectoryName(pathToExe);

                    Directory.SetCurrentDirectory(pathToContentRoot);
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

            var connectionString = GetConnectionString(configuration);

            if(string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string was not found.");

            logConfig
                .ReadFrom.Configuration(configuration)
                .ConfigureSurveySolutionsLogging(Directory.GetCurrentDirectory(), "export-service")
                .Enrich.WithProperty("workerId", "root")
                .WriteTo.Postgres(connectionString);

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

        private static string? GetConnectionString(IConfiguration configuration)
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

                        c.AddIniFile($"appsettings.DEV_DEFAULTS.ini", true);


                        c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                        c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);

                        c.AddJsonFile($"appsettings.Cloud.json", true);
                        c.AddIniFile($"appsettings.Cloud.ini", true);

                        c.AddIniFile($"appsettings.Development.ini", true);
                        c.AddJsonFile($"appsettings.Production.json", true);
                        c.AddIniFile($"appsettings.Production.ini", true);

                        c.AddEnvironmentVariables("Export_");
                        c.AddCommandLine(args);
                    });

                    web.UseSerilog();
                });
        }
    }
}
