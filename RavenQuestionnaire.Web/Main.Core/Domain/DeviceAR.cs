using System;
using Main.Core.Events.Synchronization;
using Ncqrs.Domain;

namespace Main.Core.Domain
{
    public class DeviceAR : AggregateRootMappedByConvention
    {
        private DateTime registeredDate;

        private DateTime modificationDate;

        private Guid idForRegistration = Guid.Empty;

        private Guid registrator = Guid.Empty;

        private byte[] secretKey;

        private string description;

        public DeviceAR()
        {
        }

        public DeviceAR(Guid publicKey, string description, byte[] secretKey, Guid idForRegistration, Guid registrator)
            : base(publicKey)
        {
            this.ApplyEvent(
               new NewDeviceRegistered
               {
                   Registrator = registrator,
                   IdForRegistration = idForRegistration,
                   RegisteredDate = DateTime.Now,
                   SecretKey = secretKey,
                   Description = description                   
               });
        }
     
        public void UpdateDevice(string description, byte[] secretKey, Guid registrator)
        {
            this.ApplyEvent(
              new UpdateRegisteredDevice
              {
                  Registrator = registrator,
                  DeviceId = idForRegistration,
                  SecretKey = secretKey,
                  Description = description
              });
        }

        protected void OnNewDeviceRegister(NewDeviceRegistered e)
        {
            this.description = e.Description;
            this.secretKey = e.SecretKey;
            this.registeredDate = e.RegisteredDate;
            this.modificationDate = e.RegisteredDate;
            this.idForRegistration = e.IdForRegistration;
            this.registrator = e.Registrator;
        }

        protected void OnUpdateRegisteredDevice(UpdateRegisteredDevice e)
        {
            this.description = e.Description;
            this.secretKey = e.SecretKey;
            this.registrator = e.Registrator;
            this.modificationDate = DateTime.Now;
        }
    }
}
