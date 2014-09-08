using System;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    internal class EventMetada
    {
        public string Origin { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid EventSourceId { get; set; }
    }
}