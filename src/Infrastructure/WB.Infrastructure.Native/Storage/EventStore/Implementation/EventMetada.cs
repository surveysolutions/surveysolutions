using System;

namespace WB.Infrastructure.Native.Storage.EventStore.Implementation
{
    internal class EventMetada
    {
        public string Origin { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid EventSourceId { get; set; }
        public long GlobalSequence { get; set; }
    }
}