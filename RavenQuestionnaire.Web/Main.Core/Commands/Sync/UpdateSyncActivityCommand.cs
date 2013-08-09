using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncActivityAR), "UpdateSyncActivity")]
    public class UpdateSyncActivityCommand  : CommandBase
    {
        [AggregateRootId]
        public Guid Id { get; set; }

        public DateTime LastChangeSyncPoint { set; get; }

        public Guid LastSyncItem { set; get; }

        public UpdateSyncActivityCommand(Guid id, DateTime lastChangeSyncPoint, Guid lastSyncItem)
        {
            Id = id;
            LastSyncItem = lastSyncItem;
            LastChangeSyncPoint = lastChangeSyncPoint;
        }
    }
}
