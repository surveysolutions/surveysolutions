﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Masking.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using WB.Infrastructure.AspNetCore;
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

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureSurveySolutionsLogging("export-service", (host, logConfig) =>
                    logConfig
                        .Enrich.WithProperty("workerId", "root")
                        .Destructure.ByMaskingProperties("Password", "ArchivePassword")
                        .WriteTo.Postgres(host.Configuration.GetConnectionString("DefaultConnection"))
                )
                .ConfigureSurveySolutionsAppConfiguration<Startup>("Export_", args, (host, c) =>
                {
                    c.AddJsonFile($"appsettings.{Environment.MachineName}.json", true);
                    c.AddJsonFile($"appsettings.Cloud.json", true);
                    c.AddJsonFile($"appsettings.Production.json", true);
                })
                .ConfigureWebHostDefaults(web =>
                {
                    if (!args.Contains("--kestrel"))
                    {
                        web.UseHttpSys();
                    }

                    web.UseStartup<Startup>();
                   // web.UseSerilog();
                });
        }
    }
}
