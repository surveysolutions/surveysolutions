using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(SyncActivityAR))]
    public class CreateSyncActivityCommand : CommandBase
    {
        public Guid publicKey { get; set; }

        public Guid ClientDeviceId { get; set; }
    
        /*public Guid ClientInstanceKey;*/
        
        public CreateSyncActivityCommand(Guid id, Guid deviceId/*, Guid clientInstanceKey*/)
        {
            this.publicKey = id;
            this.ClientDeviceId = deviceId;
            //this.ClientInstanceKey = clientInstanceKey;
        }
    }
}
