using System;
using Main.Core.Events.Sync;
using Ncqrs;
using Ncqrs.Domain;

namespace WB.Core.Synchronization.Aggregates
{
    public class ClientDeviceAR : AggregateRootMappedByConvention
    {
        private IClock clock = NcqrsEnvironment.Get<IClock>();

        private Guid publicKey;
        
        private long lastSyncItemIdentifier;

        public ClientDeviceAR()
        {
        }

        public ClientDeviceAR(Guid Id, string deviceId, Guid clientInstanceKey, Guid SupervisorKey)
            : base(Id)
        {
            this.CreateClientDevice(Id, deviceId, clientInstanceKey, SupervisorKey);
        }

        public void CreateClientDevice(Guid id, string deviceId, Guid clientInstanceKey, Guid supervisorKey)
        {
            base.ApplyEvent(new NewClientDeviceCreated()
            {
                Id = id,
                CreationDate = this.clock.UtcNow(),
                DeviceId = deviceId,
                ClientInstanceKey = clientInstanceKey,
                SupervisorKey = supervisorKey
            });
        }

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
            this.publicKey = evt.Id;
            this.lastSyncItemIdentifier = 0;
        }

        public void UpdatelastSyncItemIdentifier(long newLastSyncItemIdentifier)
        {
            if(newLastSyncItemIdentifier < this.lastSyncItemIdentifier)
                throw new ArgumentException("Last update identifier can't be less then current");

            base.ApplyEvent(new ClientDeviceLastSyncItemUpdated()
            {
                Id = this.publicKey,
                ChangeDate = this.clock.UtcNow(),
                LastSyncItemSequence = newLastSyncItemIdentifier});
        }

        protected void OnClientDeviceLastSyncItemUpdated(ClientDeviceLastSyncItemUpdated evt)
        {
            this.lastSyncItemIdentifier = evt.LastSyncItemSequence;
        }
    }
}
