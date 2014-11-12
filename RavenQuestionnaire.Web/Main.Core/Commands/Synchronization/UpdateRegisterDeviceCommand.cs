using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(DeviceAR), "UpdateDevice")]
    public class UpdateRegisterDeviceCommand : CommandBase
    {
        public UpdateRegisterDeviceCommand(string description, Guid publicKey, byte[] secretKey, Guid registrator)
        {
            this.PublicKey = publicKey;
            this.Registrator = registrator;
            this.Description = description;
            this.SecretKey = secretKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public Guid Registrator { get; set; }

        public string Description { get; set; }

        public byte[] SecretKey { get; set; }
    }
}
