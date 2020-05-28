using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Aggregates
{
    public class AggregateRootCacheItem
    {
        public AggregateRootCacheItem(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; }

        public IEventSourcedAggregateRoot AggregateRoot { get; set; }
        public IEnumerable<CommittedEvent> Events { get; set; }
        public PrototypeType? PrototypeType { get; set; }

        public AggregateRootCacheItem SetPrototypeType(PrototypeType? prototypeType)
        {
            PrototypeType = prototypeType;
            return this;
        }

        public AggregateRootCacheItem SetEvents(IEnumerable<CommittedEvent> events)
        {
            Events = events;
            return this;
        }
    }
}
