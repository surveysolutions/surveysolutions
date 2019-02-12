using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using WB.Services.Infrastructure.Logging;

namespace WB.Services.Export.Host
{
    class Program
    {
        private static FileStream pid;

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

                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                args = args.Where(arg => arg != "--console").ToArray();

                var useKestrel = args.Contains("--kestrel");
                args = args.Where(arg => arg != "--kestrel").ToArray();

                string currentWorkingDir = Directory.GetCurrentDirectory();

                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                    currentWorkingDir = pathToContentRoot;
                    Directory.SetCurrentDirectory(currentWorkingDir);
                }

                var builder = CreateWebHostBuilder(args, useKestrel);

                if (isService)
                {
                    builder.UseContentRoot(currentWorkingDir);
                }

                OpenPIDFile();

                var host = builder.Build();

                if (isService)
                {
                    host.RunAsCustomService();
                }
                else
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
        }

        private static void ConfigureSerilog(LoggerConfiguration logConfig, IConfiguration configuration)
        {
            var seqConfig = configuration.GetSection("Logging:Seq");

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            var fileLog = Path.Combine(Directory.GetCurrentDirectory(), "..", "logs", "export-service.log");

            logConfig
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("AppType", "ExportService")
                .Enrich.WithProperty("Version", fvi.FileVersion)
                .Enrich.WithProperty("VersionInfo", fvi.ProductVersion)
                .Enrich.WithProperty("Host", Environment.MachineName)
                .WriteTo.Postgres(configuration.GetConnectionString("DefaultConnection"), LogEventLevel.Error)
                .WriteTo
                    .File(Path.GetFullPath(fileLog), 
                        rollingInterval: RollingInterval.Day);

            var hook = configuration.GetSection("Slack").GetValue<string>("Hook");
            if (!string.IsNullOrWhiteSpace(hook))
            {
                logConfig = logConfig.WriteTo.Slack(hook, LogEventLevel.Fatal);
            }

            var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

            foreach (var assemblyMetadataAttribute in metadata)
            {
                logConfig.Enrich.WithProperty(assemblyMetadataAttribute.Key, assemblyMetadataAttribute.Value);
            }

            if (!string.IsNullOrWhiteSpace(seqConfig["Uri"]))
            {
                var apiKey = seqConfig["ApiKey"];
                logConfig.WriteTo.Seq(seqConfig["Uri"], apiKey: apiKey, restrictedToMinimumLevel: LogEventLevel.Verbose);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, bool useKestrel)
        {
            var host = WebHost.CreateDefaultBuilder(args);

            host.ConfigureAppConfiguration(c =>
            {
                c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                c.AddJsonFile($"appsettings.Production.json", true);
                c.AddJsonFile($"appsettings.Staging.json", true);

                c.AddCommandLine(args);
            });

            host.ConfigureLogging((hosting, logging) =>
            {
                var logConfig = new LoggerConfiguration();
                ConfigureSerilog(logConfig, hosting.Configuration);

                logConfig.Enrich.WithProperty("Environment", hosting.HostingEnvironment.EnvironmentName);

                if (hosting.HostingEnvironment.IsDevelopment())
                {
                    logConfig.WriteTo.Console(LogEventLevel.Information);
                }

                Log.Logger = logConfig.CreateLogger();

                if (!hosting.HostingEnvironment.IsDevelopment())
                {
                    logging.ClearProviders();
                }
            });

            host
                .UseSerilog()
                .UseUrls(GetCommandLineUrls(args));

            host = useKestrel ? host.UseKestrel() : host.UseHttpSys();
            return host.UseStartup<Startup>();
        }

        private static string GetCommandLineUrls(string[] args) =>
            new ConfigurationBuilder().AddCommandLine(args).Build()["urls"];

        // pid file - is a file that is exists only while process is alive and contains own process id
        private static void OpenPIDFile()
        {
            pid = new FileStream("pid", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096,
                FileOptions.DeleteOnClose);
            var writer = new StreamWriter(pid);

            writer.WriteLine(Process.GetCurrentProcess().Id);
            writer.Flush();
        }
    }
}
