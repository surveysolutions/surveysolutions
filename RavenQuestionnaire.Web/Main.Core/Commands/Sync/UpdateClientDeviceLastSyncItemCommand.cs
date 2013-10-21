using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
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
