using System;
using Main.Core.Domain;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(SyncActivityAR))]
    public class CreateSyncActivityCommand
    {
        public Guid Id;
        public Guid ClientDeviceId;
        
        public Guid ClientInstanceKey;
        
        public CreateSyncActivityCommand(Guid id, Guid deviceId, Guid clientInstanceKey)
        {
            this.Id = id;
            this.ClientDeviceId = deviceId;
            this.ClientInstanceKey = clientInstanceKey;


        }
    }
}
