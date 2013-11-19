using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(DeviceAR))]
    public class RegisterNewDeviceCapiCommand : CommandBase
    {
        public RegisterNewDeviceCapiCommand(string description, byte[] publicKeySupervisor, Guid tabletId)
        {
            this.Description = description;
            this.SupervisorPublicKey = publicKeySupervisor;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Today;
        }

        [AggregateRootId]
        public Guid RegisterGuid { get; set; }

        public string Description { get; set; }

        public byte[] SupervisorPublicKey { get; set; }

        public DateTime RegisteredDate { get; set; }

        public Guid TabletId { get; set; }
    }
}
