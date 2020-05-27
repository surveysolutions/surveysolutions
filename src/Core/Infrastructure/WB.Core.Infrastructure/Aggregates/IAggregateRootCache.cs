using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRootCache
    {
        AggregateRootCacheItem Get(Guid id);
        void Set(IEventSourcedAggregateRoot aggregateRoot);
        AggregateRootCacheItem GetOrCreate(Guid id, Func<AggregateRootCacheItem, AggregateRootCacheItem> factory);
        
        /// <summary>
        /// Make sure item won't be evicted from cache for extended period of time
        /// </summary>
        /// <param name="id">Aggregate id</param>
        /// <param name="period">Period</param>
        void PinItem(Guid id, TimeSpan period);

        void UnpinItem(Guid id);
        void Evict(Guid id);
    }

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
