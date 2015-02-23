using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Synchronization.Aggregates;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.Implementation.SyncLogger;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization
{
    public class SynchronizationModule : NinjectModule
    {
        private readonly SyncSettings syncSettings;

        public SynchronizationModule(SyncSettings syncSettings)
        {
            this.syncSettings = syncSettings;
        }

        public override void Load()
        {
            this.Bind<ISyncLogger>().To<SyncLogger>();
            this.Bind<ISyncManager>().To<SyncManager>();
            this.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Bind<SyncSettings>().ToConstant(this.syncSettings);
            this.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();

            CommandRegistry.Setup<Tablet>()
                .InitializesWith<RegisterTabletCommand>(command => command.DeviceId, (command, aggregate) => aggregate.CreateClientDevice(command));
        }
    }
}
