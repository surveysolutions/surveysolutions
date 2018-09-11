using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WB.Services.Export.Host
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var isWorker = args.Contains("--worker");

            args = args.Where(arg => arg != "--console" && arg != "--worker").ToArray();

            if (isWorker)
            {
                var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                var hostBuilder = new HostBuilder()
                    .ConfigureHostConfiguration(c => c.AddConfiguration(config))
                    .ConfigureAppConfiguration(c => c.AddConfiguration(config))

                    .ConfigureServices((hostContext, services) =>
                    {
                        var startup = new Startup(hostContext.Configuration);
                        startup.ConfigureServices(services);
                    });

                await hostBuilder.RunConsoleAsync();
            }
            else
            {

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
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseHttpSys()
                .UseStartup<Startup>();
        }
    }
}
