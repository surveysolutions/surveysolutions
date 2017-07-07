using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMeta
    {
        public SyncItemsMeta()
        {
        }
        public SyncItemsMeta(Guid aggregateRootId, string aggregateRootType, Guid? aggregateRootPeak)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootType = aggregateRootType;
            AggregateRootPeak = aggregateRootPeak;
        }

        public Guid AggregateRootId { get; set; }
        public string AggregateRootType { get; set; }
        public Guid? AggregateRootPeak { get; set; }
    }
}