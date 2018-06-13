using System;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using NLog;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresWriteSideModule : IModule
    {
        private readonly PostgreConnectionSettings eventStoreSettings;
        private readonly DbUpgradeSettings dbUpgradeSettings;

        public PostgresWriteSideModule(PostgreConnectionSettings eventStoreSettings, DbUpgradeSettings dbUpgradeSettings)
        {
            this.eventStoreSettings = eventStoreSettings;
            this.dbUpgradeSettings = dbUpgradeSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<IStreamableEventStore, PostgresEventStore>("connectionSettings", this.eventStoreSettings);
            registry.BindToMethod<IEventStore>(context => context.Get<IStreamableEventStore>());
        }

        public Task Init(IServiceLocator serviceLocator)
        {
            try
            {
                DatabaseManagement.InitDatabase(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName);
                DbMigrationsRunner.MigrateToLatest(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName, this.dbUpgradeSettings);
            }
            catch (Exception exc)
            {
                LogManager.GetLogger("maigration", typeof(PostgresWriteSideModule)).Fatal(exc, "Error during db initialization.");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
