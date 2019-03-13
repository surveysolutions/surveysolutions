using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.UI.Designer.Migrations.PlainStore;

namespace WB.UI.Designer1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                var migrationsType = typeof(M001_Init);
                var dbUpgradeSettings = new DbUpgradeSettings(migrationsType.Assembly, migrationsType.Namespace);
                DbMigrationsRunner.MigrateToLatest(connectionString, "plainstore", dbUpgradeSettings);
            }
            webHost.Run();
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
