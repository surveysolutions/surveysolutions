#nullable enable
using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure
{
    public static class AggregateRootCacheExtensions
    {
        private const string Key = "root";

        public static void SetAggregateRoot(this IAggregateRootCache cache, IEventSourcedAggregateRoot aggregateRoot)
        {
            cache.GetOrCreate(aggregateRoot.EventSourceId).Meta[Key] = aggregateRoot;
        }

        public static void EvictAggregateRoot(this IAggregateRootCache cache, Guid aggregateRootId)
        {
            cache.GetOrCreate(aggregateRootId).Meta[Key] = null;
        }

        public static IEventSourcedAggregateRoot? GetAggregateRoot(this IAggregateRootCache cache, Guid aggregateId)
        {
            return cache.Get(aggregateId)?.Meta.GetOrNull(Key) as IEventSourcedAggregateRoot;
        }
    }
}
