using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Events.Sync
{
    [Serializable]
    public class SyncActivityUpdated
    {
        [AggregateRootId]
        public Guid Id { set; get; }

        public DateTime ChangeDate { set; get; }
    }
}