using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Synchronization.Aggregates;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.Implementation.SyncManager;
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
            this.Bind<ISyncManager>().To<SyncManager>();
            this.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Bind<SyncSettings>().ToConstant(this.syncSettings);
            this.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();

            CommandRegistry
                .Setup<Tablet>()
                .InitializesWith<RegisterTabletCommand>(command => command.DeviceId, (command, aggregate) => aggregate.CreateClientDevice(command))
                .Handles<TrackUserLinkingRequestCommand>(command => command.DeviceId, (command, aggregate) => aggregate.TrackUserLinking(command))
                .Handles<TrackArIdsRequestCommand>(command => command.DeviceId, (command, aggregate) => aggregate.TrackArIds(command))
                .Handles<TrackHandshakeCommand>(command => command.DeviceId, (command, aggregate) => aggregate.TrackHandshake(command))
                .Handles<UnlinkUserFromDeviceCommand>(command => command.DeviceId, (command, aggregate) => aggregate.UnlinkUser(command))
                .Handles<TrackPackageRequestCommand>(command => command.DeviceId, (command, aggregate) => aggregate.TrackPackageRequest(command));
        }
    }
}
