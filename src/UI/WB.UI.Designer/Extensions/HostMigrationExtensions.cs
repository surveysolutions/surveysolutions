using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.UI.Designer.Extensions
{
    public static class HostMigrationExtensions
    {
        public static IHost RunMigrations(this IHost host, Type firstMigration, string schemaName = "plainstore")
        {
            var connectionString = host.Services.GetRequiredService<IConfiguration>()["ConnectionStrings:DefaultConnection"];

            DatabaseManagement.InitDatabase(connectionString, schemaName);
            var dbUpgradeSettings = new DbUpgradeSettings(firstMigration.Assembly, firstMigration.Namespace);
            DbMigrationsRunner.MigrateToLatest(connectionString, schemaName, dbUpgradeSettings, host.Services.GetService<ILoggerProvider>());

            return host;
        }
    }
}
