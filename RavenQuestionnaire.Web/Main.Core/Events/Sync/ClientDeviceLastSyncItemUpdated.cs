using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class ClientDeviceLastSyncItemUpdated
    {
        [AggregateRootId]
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }

        public long LastSyncItemSequence { set; get; }

    }
}
