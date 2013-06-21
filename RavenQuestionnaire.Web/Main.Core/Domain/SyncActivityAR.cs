using System;
using Main.Core.Events.Sync;
using Ncqrs;

namespace Main.Core.Domain
{
    using Ncqrs.Domain;

    public class SyncActivityAR : AggregateRootMappedByConvention
    {
        private Guid publicKey;
        private Guid deviceId;
        private DateTime CreationDate;
        private DateTime LastChangeDate;

        private IClock clock = NcqrsEnvironment.Get<IClock>();


        public SyncActivityAR(Guid publicKey, Guid ClientDeviceId)
            : base(publicKey)
        {
            base.ApplyEvent(new NewSyncActivityCreated()
                {
                    PublicKey = publicKey,
                    CreationDate = clock.UtcNow(),
                    DeviceId = deviceId
                });
        }

        protected void OnNewSyncActivityCreated(NewSyncActivityCreated evt)
        {
            publicKey = evt.PublicKey;
            deviceId = evt.DeviceId;
            CreationDate = evt.CreationDate;
            LastChangeDate = evt.CreationDate;
        }

        public void UpdateSyncActivity(Guid id)
        {
            base.ApplyEvent(new SyncActivityUpdated()
            {
                Id = id,
                ChangeDate = clock.UtcNow()
            });
        }

        protected void OnSyncActivityUpdated(SyncActivityUpdated evt)
        {
            LastChangeDate = evt.ChangeDate;
        }
    }
}
