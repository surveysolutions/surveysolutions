using System.Data.Entity;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class FluentMigratorInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        private readonly string schemaName;
        private readonly DbUpgradeSettings upgradeSettings;

        public FluentMigratorInitializer(string schemaName, DbUpgradeSettings upgradeSettings)
        {
            this.schemaName = schemaName;
            this.upgradeSettings = upgradeSettings;
        }

        public void InitializeDatabase(TContext context)
        {
            try
            {
                DatabaseManagement.InitDatabase(context.Database.Connection.ConnectionString, this.schemaName);
            }
            catch (System.Exception exc)
            {
                ServiceLocator.Current.GetInstance<ILoggerProvider>().GetForType(this.GetType()).Fatal("Error during db initialization.", exc);
                throw;
            }

            DbMigrationsRunner.MigrateToLatest(context.Database.Connection.ConnectionString, this.schemaName, this.upgradeSettings);
        }
    }
}