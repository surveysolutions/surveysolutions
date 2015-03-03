using Main.Core.Events.Sync;
using Ncqrs.Domain;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.Synchronization.Aggregates
{
    public class Tablet : AggregateRootMappedByConvention
    {
        public Tablet() {}

        public void CreateClientDevice(RegisterTabletCommand command)
        {
            this.ApplyEvent(new TabletRegistered(command.AndroidId, command.AppVersion));
        }


        #region backward capability with old events

        protected void Apply(TabletRegistered @event)
        {
        }

        protected void Apply(UserLinkingRequested @event)
        {
        }

        protected void Apply(UserLinkedFromDevice @event)
        {
        }

        protected void Apply(PackageRequested @event)
        {
        }

        protected void Apply(HandshakeRequested @event)
        {
        }

        protected void Apply(PackageIdsRequested @event)
        {
        }

        protected void Apply(ClientDeviceLastSyncItemUpdated evt)
        {
        }

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
        } 
        #endregion
    }
}
