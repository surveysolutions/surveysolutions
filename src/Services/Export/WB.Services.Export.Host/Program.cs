using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace WB.Services.Export.Host
{
    class Program
    {
        private static FileStream pid;

        static async Task Main(string[] args)
        {
            OpenPIDFile();

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
                args = args.Where(arg => arg != "--console").ToArray();

                var useKestrel = args.Contains("--kestrel");
                args = args.Where(arg => arg != "--kestrel").ToArray();

                var builder = CreateWebHostBuilder(args, useKestrel);

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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, bool useKestrel)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration(c =>
                    {
                        c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                        c.AddCommandLine(args);
                    })
                    .ConfigureLogging((hosting, logging) =>
                    {
                        if (!hosting.HostingEnvironment.IsDevelopment())
                        {
                            logging.ClearProviders();
                        }
                    })
                    .UseNLog()
                    .UseUrls(GetCommandLineUrls(args))
                ;
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
