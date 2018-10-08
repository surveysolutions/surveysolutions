using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.Extensions.DependencyInjection;

namespace WB.Services.Export.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    Console.WriteLine(eventArgs.ExceptionObject.GetType().FullName);
                    Console.WriteLine(eventArgs.ExceptionObject.ToString());
                    logger.Error(eventArgs.ExceptionObject.ToString);
                };

                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                args = args.Where(arg => arg != "--console" && arg != "--worker").ToArray();

                var builder = CreateWebHostBuilder(args);

                if (isService)
                {
                    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                    builder.UseContentRoot(pathToContentRoot);
                }

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
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                    c.AddCommandLine(args);
                })
                .ConfigureLogging((hosting, logging) =>
                {
                    if (!hosting.HostingEnvironment.IsDevelopment())
                    {
                       // logging.ClearProviders();
                    }
                })
                .UseNLog()
                .UseUrls(GetCommandLineUrls(args))
                .UseHttpSys()
                .UseStartup<Startup>();
        }

        private static string GetCommandLineUrls(string[] args) =>
            new ConfigurationBuilder().AddCommandLine(args).Build()["urls"];
    }
}
