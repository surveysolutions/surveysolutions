using System;
using Main.Core.Events.Sync;
using Ncqrs;
using Ncqrs.Domain;

namespace Main.Core.Domain
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
                CreationDate = clock.UtcNow(),
                DeviceId = deviceId,
                ClientInstanceKey = clientInstanceKey,
                SupervisorKey = supervisorKey
            });
        }

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
            publicKey = evt.Id;
            lastSyncItemIdentifier = 0;
        }

        public void UpdatelastSyncItemIdentifier(long newLastSyncItemIdentifier)
        {
            if(newLastSyncItemIdentifier < lastSyncItemIdentifier)
                throw new ArgumentException("Last update identifier can't be less then current");

            base.ApplyEvent(new ClientDeviceLastSyncItemUpdated()
            {
                Id = publicKey,
                ChangeDate = clock.UtcNow(),
                LastSyncItemSequence = newLastSyncItemIdentifier});
        }

        protected void OnClientDeviceLastSyncItemUpdated(ClientDeviceLastSyncItemUpdated evt)
        {
            lastSyncItemIdentifier = evt.LastSyncItemSequence;
        }
    }
}
