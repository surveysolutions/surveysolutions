using System;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using NLog;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Resources;
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
            registry.Bind<IHeadquartersEventStore, PostgresEventStore>(new ConstructorArgument("connectionSettings", (ctx) => this.eventStoreSettings));
            registry.BindToMethod<IEventStore>(context => context.Get<IHeadquartersEventStore>());
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            try
            {
                status.Message = Modules.InitializingDb;
                DatabaseManagement.InitDatabase(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName);

                status.Message = Modules.MigrateDb;
                DbMigrationsRunner.MigrateToLatest(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName, this.dbUpgradeSettings);
            }
            catch (Exception exc)
            {
                LogManager.GetLogger("migration", typeof(PostgresWriteSideModule)).Fatal(exc, "Error during db initialization.");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
