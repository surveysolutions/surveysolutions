using Main.Core.Events.Sync;
using Ncqrs;

namespace Main.Core.Domain
{
    using System;
    using Ncqrs.Domain;

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
            base.ApplyEvent(new NewClientDeviceCreated()
                {Id = Id, 
                CreationDate = clock.UtcNow(),
                DeviceId = deviceId,
                ClientInstanceKey = clientInstanceKey,
                SupervisorKey = SupervisorKey
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
