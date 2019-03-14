using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WB.UI.Designer.Migrations.PlainStore;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer1
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateWebHostBuilder(args).Build()
                .RunMigrations(typeof(M001_Init), "plainstore")
                .Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration(c =>
                {
                    c.AddIniFile("appsettings.ini", false, true);
                    c.AddIniFile($"appsettings.{Environment.MachineName}.ini", true);
                    c.AddIniFile($"appsettings.Production.ini", true);
                    c.AddCommandLine(args);
                });
        }
    }
}
