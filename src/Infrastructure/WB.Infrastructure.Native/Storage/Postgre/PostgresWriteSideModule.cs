using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing.Storage;
using NLog;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Resources;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresWriteSideModule : IModule, IInitModule
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
            registry.BindWithConstructorArgument<IHeadquartersEventStore, PostgresEventStore>("connectionSettings", this.eventStoreSettings);
            registry.BindWithConstructorArgument<IEventStore, PostgresEventStore>("connectionSettings", this.eventStoreSettings);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            try
            {
                status.Message = Modules.InitializingDb;
                DatabaseManagement.InitDatabase(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName);

                status.Message = Modules.MigrateDb;
                DbMigrationsRunner.MigrateToLatest(this.eventStoreSettings.ConnectionString, this.eventStoreSettings.SchemaName, this.dbUpgradeSettings,
                    serviceLocator.GetInstance<ILoggerProvider>());

                status.ClearMessage();
            }
            catch (Exception exc)
            {
                status.Error(Modules.ErrorDuringRunningMigrations, exc);

                LogManager.GetLogger("migration", typeof(PostgresWriteSideModule)).Fatal(exc, "Error during db initialization.");
                throw new InitializationException(Subsystem.Database, null, exc);
            }

            return Task.CompletedTask;
        }
    }
}
