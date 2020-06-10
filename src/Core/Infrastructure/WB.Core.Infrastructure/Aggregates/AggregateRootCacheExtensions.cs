#nullable enable
using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public static class AggregateRootCacheExtensions
    {
        public static AggregateRootCacheItem? Get(this IAggregateRootCache cache, Guid aggregateId)
        {
            return cache.TryGetValue(aggregateId, out var item) ? item : null;
        }

        public static AggregateRootCacheItem GetOrCreate(this IAggregateRootCache cache, Guid aggregateId)
        {
            var cacheItem = cache.TryGetValue(aggregateId, out var item) ? item : null;
            return cacheItem ?? cache.CreateEntry(aggregateId);
        }

        public static AggregateRootCacheItem? Update(this IAggregateRootCache cache, Guid aggregateId, TimeSpan? slidingExpiration = null)
        {
            var item = cache.Get(aggregateId);

            if (item == null)
            {
                return null;
            }

            return cache.CreateEntry(aggregateId, entry =>
            {
                entry.Meta = item.Meta;
                return entry;
            }, slidingExpiration);
        }
    }
}
