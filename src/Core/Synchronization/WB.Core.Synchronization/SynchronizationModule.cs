using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Synchronization.Aggregates;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.MetaInfo;

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
            this.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Bind<SyncSettings>().ToConstant(this.syncSettings);
            this.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();

            CommandRegistry.Setup<Tablet>()
                .InitializesWith<RegisterTabletCommand>(command => command.DeviceId, (command, aggregate) => aggregate.CreateClientDevice(command));
        }
    }
}
