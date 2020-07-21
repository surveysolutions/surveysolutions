using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateCache : AggregateRootCache
    {
        private readonly IEvictionNotifier notify;

        public WebTesterAggregateCache(IMemoryCache memoryCache,
            IEvictionNotifier notify, ILogger<WebTesterAggregateCache> logger) : base(memoryCache, logger)
        {
            this.notify = notify;
        }

        protected override void CacheItemRemoved(Guid id, EvictionReason reason)
        {
            if (reason != EvictionReason.Replaced)
            {
                notify.Evict(id);
            }

            base.CacheItemRemoved(id, reason);
        }
    }
}
