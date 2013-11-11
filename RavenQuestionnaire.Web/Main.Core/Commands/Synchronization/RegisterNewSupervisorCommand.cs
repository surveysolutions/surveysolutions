using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(DeviceAR))]
    public class RegisterNewSupervisorCommand : CommandBase
    {
        public RegisterNewSupervisorCommand(string description, byte[] publicKeyCapi, Guid tabletId)
        {
            this.PublicKeyCapi = publicKeyCapi;
            this.Description = description;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Now;
        }

        [AggregateRootId]
        public Guid RegisterGuid { get; set; }

        public string Description { get; set; }

        public byte[] PublicKeyCapi { get; set; }

        public DateTime RegisteredDate { get; set; }

        public Guid TabletId { get; set; }
    }
}
