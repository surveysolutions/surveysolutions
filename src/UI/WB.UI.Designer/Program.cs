using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WB.UI.Designer1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
            webHostBuilder.ConfigureAppConfiguration(c =>
            {
                c.AddIniFile("appsettings.ini", false, true);
                c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                c.AddIniFile($"appsettings.Production.ini", true);

                c.AddCommandLine(args);
            });

            return webHostBuilder;
        }
    }
}
