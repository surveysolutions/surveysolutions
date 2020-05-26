using System;
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
            CoreMetrics.StatefullInterviewsCached?.Labels("added").Inc();
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

                    return factory != null ? factory(result) : result;
                });
            });
        }

        public void PinItem(Guid id, TimeSpan period)
        {
           // throw new NotImplementedException();
        }

        public void UnpinItem(Guid id)
        {
           // throw new NotImplementedException();
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (state is AggregateRootCacheItem cacheItem)
            {
                CacheItemRemoved(cacheItem.Id, reason);
            }
        }

        protected virtual void CacheItemRemoved(Guid id, EvictionReason reason)
        {
            CoreMetrics.StatefullInterviewsCached?.Labels("removed").Inc();
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
