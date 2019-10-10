using Microsoft.EntityFrameworkCore;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class FluentMigratorInitializer
    {
        private readonly string connectionString;
        private readonly string schemaName;
        private readonly DbUpgradeSettings upgradeSettings;

        public FluentMigratorInitializer(string connectionString, string schemaName, DbUpgradeSettings upgradeSettings)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
            this.upgradeSettings = upgradeSettings;
        }

        public void InitializeDatabase()
        {
            try
            {
                DatabaseManagement.InitDatabase(connectionString, this.schemaName);
            }
            catch (System.Exception exc)
            {
                ServiceLocator.Current.GetInstance<ILoggerProvider>().GetForType(this.GetType()).Fatal("Error during db initialization.", exc);
                throw;
            }

            DbMigrationsRunner.MigrateToLatest(connectionString, this.schemaName, this.upgradeSettings);
        }
    }
}
