using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.UI.Designer1.Extensions
{
    public static class HostMigrationExtensions
    {
        public static IWebHost RunMigrations(this IWebHost host, Type firstMigration, string schemaName = "plainstore")
        {
            var connectionString = host.Services.GetService<IConfiguration>()["ConnectionStrings:DefaultConnection"];

            DatabaseManagement.InitDatabase(connectionString, schemaName);
            var dbUpgradeSettings = new DbUpgradeSettings(firstMigration.Assembly, firstMigration.Namespace);
            DbMigrationsRunner.MigrateToLatest(connectionString, schemaName, dbUpgradeSettings);

            return host;
        }
    }
}
