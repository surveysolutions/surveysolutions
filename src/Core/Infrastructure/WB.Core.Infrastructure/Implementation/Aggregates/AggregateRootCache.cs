 using System;
 using System.Security.Cryptography.X509Certificates;
 using Microsoft.Extensions.Caching.Memory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Metrics;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateRootCache : IAggregateRootCache
    {
        private readonly IAggregateLock aggregateLock;
        private readonly IMemoryCache memoryCache;

        public AggregateRootCache(IAggregateLock aggregateLock, IMemoryCache memoryCache)
        {
            this.aggregateLock = aggregateLock;
            this.memoryCache = memoryCache;
        }

        protected virtual TimeSpan Expiration => TimeSpan.FromMinutes(5);

        public void Set(IEventSourcedAggregateRoot aggregateRoot)
        {
            var cacheItem = GetOrCreate(aggregateRoot.EventSourceId);
            cacheItem.AggregateRoot = aggregateRoot;
        }

        public AggregateRootCacheItem GetOrCreate(Guid id, Func<AggregateRootCacheItem, AggregateRootCacheItem> factory = null)
        {
            return this.aggregateLock.RunWithLock(id.FormatGuid(), () =>
            {
                return this.memoryCache.GetOrCreate(Key(id), item =>
                {
                    item.SetSlidingExpiration(Expiration)
                        .RegisterPostEvictionCallback(CacheItemRemoved);

                    var result = new AggregateRootCacheItem(id);
                    CoreMetrics.StatefullInterviewsCached?.Labels("added").Inc();
                    return factory != null ? factory(result) : result;
                });
            });
        }

        private static readonly TimeSpan MaxPinPeriod = TimeSpan.FromHours(1);

        public void PinItem(Guid id, TimeSpan period)
        {
            var item = this.Get(id);
            if (item != null)
            {
                var expiration = period > MaxPinPeriod ? MaxPinPeriod : period;

                CoreMetrics.StatefullInterviewsCached?.Labels("added").Inc();
                this.memoryCache.Set(Key(id), item, new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(expiration)
                        .RegisterPostEvictionCallback(CacheItemRemoved));
            }
        }

        public void UnpinItem(Guid id)
        {
           PinItem(id, Expiration);
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (value is AggregateRootCacheItem cacheItem)
            {
                CoreMetrics.StatefullInterviewsCached?.Labels("removed").Inc();
                CacheItemRemoved(cacheItem.Id, reason);
            }
        }

        protected virtual void CacheItemRemoved(Guid id, EvictionReason reason)
        {
            
        }

        protected virtual string Key(Guid id) => "ar_" + id;

        public void Evict(Guid aggregateId)
        {
            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () => memoryCache.Remove(Key(aggregateId)));
        }

        public AggregateRootCacheItem Get(Guid id)
        {
            if (this.memoryCache.TryGetValue(Key(id), out AggregateRootCacheItem cacheItem))
            {
                return cacheItem;
            }

            return null;
        }
    }
}
