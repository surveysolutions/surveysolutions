using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Metrics;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateRootCache : IAggregateRootCache
    {
        private readonly IMemoryCache memoryCache;
        private CancellationTokenSource resetCacheToken = new CancellationTokenSource();

        public AggregateRootCache(IMemoryCache memoryCache, 
            AggregateRootCacheExpirationSettings settings = null)
        {
            this.memoryCache = memoryCache;
            Expiration = settings?.Expiration ?? TimeSpan.FromMinutes(25);
            MaxCacheExpiration = settings?.MaxCacheExpiration ?? MaxCachePeriod;
        }

        protected virtual TimeSpan Expiration { get; }
        protected virtual TimeSpan MaxCacheExpiration { get; } 
        private static readonly TimeSpan MaxCachePeriod = TimeSpan.FromHours(1);
        

        protected virtual string Key(Guid id) => "ar::" + id;

        public bool TryGetValue(Guid aggregateId, out AggregateRootCacheItem value)
        {
            return this.memoryCache.TryGetValue(Key(aggregateId), out value);
        }

        public AggregateRootCacheItem CreateEntry(Guid aggregateId, Func<AggregateRootCacheItem, AggregateRootCacheItem> factory = null, TimeSpan? expirationPeriod = null)
        {
            expirationPeriod ??= Expiration;

            TimeSpan expiration = expirationPeriod > MaxCacheExpiration ? MaxCacheExpiration : expirationPeriod.Value;
            var value = new AggregateRootCacheItem(aggregateId);

            value = factory == null ? value : factory(value);

            this.memoryCache.Set(Key(aggregateId), value, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(expiration)
                .RegisterPostEvictionCallback(CacheItemRemoved)
                .AddExpirationToken(new CancellationChangeToken(resetCacheToken.Token)));
            
            CoreMetrics.StatefullInterviewsCached?.Labels("added").Inc();

            return value;
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (value is AggregateRootCacheItem cacheItem)
            {
                CacheItemRemoved(cacheItem.Id, reason);
            }

            CoreMetrics.StatefullInterviewsCached?.Labels("removed").Inc();
        }

        protected virtual void CacheItemRemoved(Guid id, EvictionReason reason)
        {
        }

        public void Evict(Guid aggregateId)
        {
            this.memoryCache.Remove(Key(aggregateId));
        }

        public void Clear()
        {
            if (resetCacheToken != null && !resetCacheToken.IsCancellationRequested && resetCacheToken.Token.CanBeCanceled)
            {
                resetCacheToken.Cancel();
                resetCacheToken.Dispose();
            }

            resetCacheToken = new CancellationTokenSource();
        }
    }

    public class AggregateRootCacheExpirationSettings
    {
        public TimeSpan? Expiration { get; set; }
        public TimeSpan? MaxCacheExpiration { get; set; }

        public AggregateRootCacheExpirationSettings(TimeSpan? expiration = null, TimeSpan? maxCacheExpiration = null)
        {
            this.Expiration = expiration;
            this.MaxCacheExpiration = maxCacheExpiration;
        }
    }
}
