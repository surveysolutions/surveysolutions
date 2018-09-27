using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WB.Services.Export.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine(eventArgs.ExceptionObject.GetType().FullName);
                Console.WriteLine(eventArgs.ExceptionObject.ToString());
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                    c.AddCommandLine(args);
                })
                .UseUrls(GetCommandLineUrls(args))
                .UseHttpSys()
                .UseStartup<Startup>();
        }

        private static string GetCommandLineUrls(string[] args) =>
            new ConfigurationBuilder().AddCommandLine(args).Build()["urls"];
    }
}
