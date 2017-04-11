using System.Reflection;
using FluentMigrator;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class DbUpgradeSettings
    {
        public DbUpgradeSettings(Assembly migrationsAssembly, string migrationsNamespace)
        {
            this.MigrationsAssembly = migrationsAssembly;
            this.MigrationsNamespace = migrationsNamespace;
        }

        public Assembly MigrationsAssembly { get; private set; }

        public string MigrationsNamespace { get; private set; }

        public static DbUpgradeSettings FromFirstMigration<T>() where T: Migration
        {
            return new DbUpgradeSettings(typeof(T).Assembly, typeof(T).Namespace);
        }
    }
}