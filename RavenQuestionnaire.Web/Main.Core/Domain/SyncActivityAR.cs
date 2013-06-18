using System;
using Main.Core.Events.Sync;
using Ncqrs;

namespace Main.Core.Domain
{
    using Ncqrs.Domain;

    public class SyncActivityAR : AggregateRootMappedByConvention
    {
        private Guid Id;
        private Guid deviceId;
        private DateTime CreationDate;
        private DateTime LastChangeDate;

        private IClock clock = NcqrsEnvironment.Get<IClock>();


        public SyncActivityAR(Guid id, Guid deviceId)
            : base(id)
        {
            base.ApplyEvent(new NewSyncActivityCreated()
                {
                    Id = id,
                    CreationDate = clock.UtcNow(),
                    DeviceId = deviceId
                });
        }

        protected void OnNewSyncActivityCreated(NewSyncActivityCreated evt)
        {
            Id = evt.Id;
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
