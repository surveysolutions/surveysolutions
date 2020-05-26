using System;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateCache : AggregateRootCache
    {
        private readonly IEvictionNotifier notify;

        public WebTesterAggregateCache(
            IAggregateLock aggregateLock, 
            IMemoryCache memoryCache,
            IEvictionNotifier notify) : base(aggregateLock, memoryCache)
        {
            this.notify = notify;
        }

        protected override void CacheItemRemoved(Guid id, EvictionReason reason)
        {
            notify.Evict(id);
            base.CacheItemRemoved(id, reason);
        }
    }
}
