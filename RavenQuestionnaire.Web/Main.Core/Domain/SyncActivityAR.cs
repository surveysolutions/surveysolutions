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


        public SyncActivityAR(Guid id, Guid deviceId)
            : base(id)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

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
        }
 
    }
}
