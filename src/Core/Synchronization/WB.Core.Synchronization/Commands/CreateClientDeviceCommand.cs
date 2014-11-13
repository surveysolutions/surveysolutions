using System;
using Ncqrs.Commanding;

namespace WB.Core.Synchronization.Commands
{
    [Serializable]
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
