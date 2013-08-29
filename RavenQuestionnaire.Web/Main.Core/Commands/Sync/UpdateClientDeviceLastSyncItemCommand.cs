using Ncqrs.Commanding;

namespace Main.Core.Commands.Sync
{
    using System;
    using Domain;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
    
    [Serializable]
    [MapsToAggregateRootMethod(typeof(ClientDeviceAR), "UpdatelastSyncItemIdentifier")]
    public class UpdateClientDeviceLastSyncItemCommand : CommandBase
    {
        [AggregateRootId]
        public Guid Id { get; set; }

        public long NewLastSyncItemIdentifier { set; get; }

        public UpdateClientDeviceLastSyncItemCommand(Guid deviceId, long changeTracker)
        {
            this.Id = deviceId;
            NewLastSyncItemIdentifier = changeTracker;
        }
    }
}
