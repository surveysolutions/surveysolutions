using Main.Core.Events.Sync;
using Ncqrs.Domain;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.Synchronization.Aggregates
{
    public class Tablet : AggregateRootMappedByConvention
    {
        public Tablet() {}

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
        }

        public void CreateClientDevice(RegisterTabletCommand command)
        {
            this.ApplyEvent(new TabletRegistered(command.AndroidId, command.AppVersion));
            this.ApplyEvent(new UserLinkingRequested(command.UserId));
        }

        public void TrackUserLinking(TrackUserLinkingRequestCommand command)
        {
            this.ApplyEvent(new UserLinkingRequested(command.UserId));
        }

        public void TrackArIds(TrackArIdsRequestCommand command)
        {
            this.ApplyEvent(new PackageIdsRequested(command.UserId, command.PackageType, command.LastSyncedPackageId, command.UpdateFromLastPakage));
        }

        public void TrackPackageRequest(TrackPackageRequestCommand command)
        {
            this.ApplyEvent(new PackageRequested(command.UserId, command.PackageType, command.PackageId));
        }

        public void TrackHandshake(TrackHandshakeCommand command)
        {
            this.ApplyEvent(new HandshakeRequested(command.UserId, command.AppVersion));
        }

        public void UnlinkUser(UnlinkUserFromDeviceCommand command)
        {
            this.ApplyEvent(new UserLinkedFromDevice(command.UserId));
        }

        protected void Apply(PackageRequested @event)
        {
        }

        protected void Apply(UserLinkedFromDevice @event)
        {
        }

        protected void Apply(HandshakeRequested @event)
        {
        }

        protected void Apply(PackageIdsRequested @event)
        {
        }

        protected void Apply(UserLinkingRequested @event)
        {
        }

        protected void Apply(ClientDeviceLastSyncItemUpdated evt)
        {
        }
    }
}
