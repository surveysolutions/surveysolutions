#nullable enable
using System;

namespace WB.Core.Infrastructure.Aggregates
{
    public interface IAggregateRootCache
    {
        /// <summary>
        /// Gets the aggregate root associated with this UUID if present.
        /// </summary>
        /// <param name="aggregateId">An UUID identifying the requested aggregate root.</param>
        /// <param name="value">The located aggregate root or null.</param>
        /// <returns>True if the key was found.</returns>
        bool TryGetValue(Guid aggregateId, out AggregateRootCacheItem value);

        /// <summary>
        /// Create or overwrite an aggregate root in the cache.
        /// </summary>
        /// <param name="aggregateId">An objectUUID identifying the aggregate root.</param>
        /// <param name="factory">Aggregate root cache item factory</param>
        /// <param name="expirationPeriod">Sliding expiration period. Default is 5 minutes</param>
        /// <returns>The newly created <see cref="AggregateRootCacheItem"/> instance.</returns>
        AggregateRootCacheItem CreateEntry(Guid aggregateId, Func<AggregateRootCacheItem, AggregateRootCacheItem>? factory = null, TimeSpan? expirationPeriod = null);

        /// <summary>
        /// Removes the aggregate root associated with the given UUID.
        /// </summary>
        /// <param name="aggregateId">An aggregate root UUID identifying the entry.</param>
        void Evict(Guid aggregateId);

        /// <summary>
        /// Clear all data in cache
        /// </summary>
        void Clear();
    }
}
