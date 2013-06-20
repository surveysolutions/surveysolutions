using Main.Core.Events.Sync;
using Ncqrs;

namespace Main.Core.Domain
{
    using System;
    using Ncqrs.Domain;

    public class ClientDeviceAR : AggregateRootMappedByConvention
    {
        private IClock clock = NcqrsEnvironment.Get<IClock>();

        private Guid id;

        private string deviceId;

        private DateTime registeredDate;

        private DateTime modificationDate;

        private Guid clientInstanceKey;

        private long lastSyncItemIdentifier;
        
        public ClientDeviceAR(Guid Id, string deviceId, Guid clientInstanceKey, string deviceType)
            : base(Id)
        {
            base.ApplyEvent(new NewClientDeviceCreated()
                {Id = Id, 
                CreationDate = clock.UtcNow(),
                DeviceId = deviceId,
                ClientInstanceKey = clientInstanceKey});
        }

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
            id = evt.Id;
            deviceId = evt.DeviceId;
            registeredDate = evt.CreationDate;
            modificationDate = evt.CreationDate;
            clientInstanceKey = evt.ClientInstanceKey;
            lastSyncItemIdentifier = 0;
        }

        public void UpdatelastSyncItemIdentifier(long newLastSyncItemIdentifier)
        {
            if(newLastSyncItemIdentifier < lastSyncItemIdentifier)
                throw new ArgumentException("Last update identifier can't be less then current");

            base.ApplyEvent(new ClientDeviceLastSyncItemUpdated()
                {Id = id,
                ChangeDate = clock.UtcNow(),
                LastSyncItemSequence = newLastSyncItemIdentifier});
        }

        protected void OnClientDeviceLastSyncItemUpdated(ClientDeviceLastSyncItemUpdated evt)
        {
            lastSyncItemIdentifier = evt.LastSyncItemSequence;
        }

    }
}
