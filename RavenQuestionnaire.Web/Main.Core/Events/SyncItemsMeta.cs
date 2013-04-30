using System;

namespace Main.Core.Events
{
    public class SyncItemsMeta
    {
        public SyncItemsMeta()
        {
        }
        public SyncItemsMeta(Guid aggregateRootId, string aggregateRootType, Guid aggregateRootPeak)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootType = aggregateRootType;
            AggregateRootPeak = aggregateRootPeak;
        }

        public Guid AggregateRootId { get; set; }
        public string AggregateRootType { get; set; }
        public Guid AggregateRootPeak { get; set; }
    }
}