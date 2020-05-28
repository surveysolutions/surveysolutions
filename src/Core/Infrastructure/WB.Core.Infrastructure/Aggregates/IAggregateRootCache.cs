using System;

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

    class AggregateRootCache : IAggregateRootCache
    {
        private AggregateRootCacheItem cacheItem = null;

        public AggregateRootCacheItem Get(Guid id)
        {
            return cacheItem;
        }

        public void Set(IEventSourcedAggregateRoot aggregateRoot)
        {
            cacheItem = new AggregateRootCacheItem(aggregateRoot.EventSourceId)
            {
                AggregateRoot = aggregateRoot
            };
        }

        public AggregateRootCacheItem GetOrCreate(Guid id, Func<AggregateRootCacheItem, AggregateRootCacheItem> factory)
        {
            return 
        }

        public void PinItem(Guid id, TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public void UnpinItem(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Evict(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
