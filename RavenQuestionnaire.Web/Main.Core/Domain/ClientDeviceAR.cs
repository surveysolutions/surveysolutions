using Main.Core.Events.Sync;
using Ncqrs;

namespace Main.Core.Domain
{
    using System;
    using Ncqrs.Domain;

    public class ClientDeviceAR : AggregateRootMappedByConvention
    {
        private Guid Id;

        private string deviceId;

        private DateTime registeredDate;

        private DateTime modificationDate;

        private Guid clientInstanceKey;

        
        public ClientDeviceAR(Guid Id, string deviceId, Guid clientInstanceKey, string deviceType)
            : base(Id)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            base.ApplyEvent(new NewClientDeviceCreated()
                {Id = Id, 
                CreationDate = clock.UtcNow(),
                DeviceId = deviceId,
                ClientInstanceKey = clientInstanceKey});
        }

        protected void OnNewClientDeviceCreated(NewClientDeviceCreated evt)
        {
            Id = evt.Id;
            deviceId = evt.DeviceId;
            registeredDate = evt.CreationDate;
            modificationDate = evt.CreationDate;
            clientInstanceKey = evt.ClientInstanceKey;
        }

        public void UpdateClientDevice()
        {
        }

    }
}
