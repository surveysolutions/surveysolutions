using System;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresWriteSideModule : NinjectModule
    {
        private readonly PostgreConnectionSettings eventStoreSettings;
        private readonly DbUpgradeSettings dbUpgradeSettings;

        public PostgresWriteSideModule(PostgreConnectionSettings eventStoreSettings, DbUpgradeSettings dbUpgradeSettings)
        {
            this.eventStoreSettings = eventStoreSettings;
            this.dbUpgradeSettings = dbUpgradeSettings;
        }

        public override void Load()
        {
            try
            {
                DatabaseManagement.InitDatabase(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName);
                DbMigrationsRunner.MigrateToLatest(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName, this.dbUpgradeSettings);
            }
            catch (Exception exc)
            {
                this.Kernel.Get<ILogger>().Fatal("Error during db initialization.", exc);
                throw;
            }


            this.Kernel.Bind<IStreamableEventStore>().To<PostgresEventStore>().InSingletonScope().WithConstructorArgument("connectionSettings", this.eventStoreSettings);
            this.Kernel.Bind<IEventStore>().ToMethod(context => context.Kernel.Get<IStreamableEventStore>());
        }
    }
}