using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Metrics;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateRootCache : IAggregateRootCache
    {
        private readonly IMemoryCache memoryCache;
        private readonly ILogger log;

        public AggregateRootCache(IMemoryCache memoryCache, ILogger log)
        {
            this.memoryCache = memoryCache;
            this.log = log;
        }

        protected virtual TimeSpan Expiration { get; } = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MaxCachePeriod = TimeSpan.FromHours(1);

        protected virtual string Key(Guid id) => "ar::" + id;

        public bool TryGetValue(Guid aggregateId, out AggregateRootCacheItem value)
        {
            return this.memoryCache.TryGetValue(Key(aggregateId), out value);
        }

        public AggregateRootCacheItem CreateEntry(Guid aggregateId, Func<AggregateRootCacheItem, AggregateRootCacheItem> factory = null, TimeSpan? expirationPeriod = null)
        {
            expirationPeriod ??= Expiration;

            TimeSpan expiration = expirationPeriod > MaxCachePeriod ? MaxCachePeriod : expirationPeriod.Value;
            var value = new AggregateRootCacheItem(aggregateId);

            value = factory == null ? value : factory(value);

            this.memoryCache.Set(Key(aggregateId), value, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(expiration)
                .RegisterPostEvictionCallback(CacheItemRemoved));
            
            CoreMetrics.StatefullInterviewsCached?.Labels("added").Inc();
            log.Debug($"Cache item added. {aggregateId}. Total: {CoreMetrics.StatefullInterviewsCached?.Labels("added").Value}");

            return value;
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (value is AggregateRootCacheItem cacheItem)
            {
                CacheItemRemoved(cacheItem.Id, reason);
            }

            CoreMetrics.StatefullInterviewsCached?.Labels("removed").Inc();
            log.Debug($"Cache item removed. {key}. Reason: {reason}. Total: {CoreMetrics.StatefullInterviewsCached?.Labels("removed").Value}");
        }

        protected virtual void CacheItemRemoved(Guid id, EvictionReason reason)
        {
        }

        public void Evict(Guid aggregateId)
        {
            this.memoryCache.Remove(Key(aggregateId));
        }
    }
}
