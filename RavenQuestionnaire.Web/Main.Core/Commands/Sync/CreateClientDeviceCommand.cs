using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(ClientDeviceAR))]
    public class CreateClientDeviceCommand  : CommandBase
    {
        public Guid Id { get; set; }
    
        public string DeviceId { get; set; }
    
        public Guid ClientInstanceKey { get; set; }

        public Guid SupervisorKey { get; set; }
        
        public CreateClientDeviceCommand(Guid id, string deviceId, Guid clientInstanceKey, Guid supervisorKey)
        {
            this.Id = id;
            this.DeviceId = deviceId;
            this.ClientInstanceKey = clientInstanceKey;
            this.SupervisorKey = supervisorKey;
        }
    }
}
