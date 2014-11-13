using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Synchronization.Aggregates;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncProvider;
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
            this.Bind<ISyncManager>().To<SyncManager>();
            this.Bind<ISyncProvider>().To<SyncProvider.SyncProvider>();
            this.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Bind<SyncSettings>().ToConstant(this.syncSettings);

            this.Bind<ISynchronizationDataStorage>().To<SimpleSynchronizationDataStorage>().InSingletonScope();
            this.Bind<IChunkWriter>().To<ReadSideChunkWriter>().InSingletonScope();
            this.Bind<IChunkReader>().To<ReadSideChunkReader>();
            this.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();

            CommandRegistry
                .Setup<ClientDeviceAR>()
                .InitializesWith<CreateClientDeviceCommand>(command => command.Id, (command, aggregate) => aggregate.CreateClientDevice(command.Id, command.DeviceId, command.ClientInstanceKey, command.SupervisorKey));
        }
    }
}
