using System.Reflection;

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
    }
}