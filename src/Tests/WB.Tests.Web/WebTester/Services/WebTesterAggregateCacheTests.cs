using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Web.WebTester.Services
{
    [TestFixture]
    [TestOf(typeof(WebTesterAggregateCache))]
    internal class WebTesterAggregateCacheTests
    {
        [Test]
        public void when_cache_entry_is_removed_should_not_notify_eviction()
        {
            var interviewId = Guid.NewGuid();
            var notifier = new Mock<IEvictionNotifier>();
            var cache = new TestableWebTesterAggregateCache(Mock.Of<IMemoryCache>(), notifier.Object);

            cache.NotifyCacheItemRemoved(interviewId, EvictionReason.Removed);

            notifier.Verify(x => x.Evict(interviewId), Times.Never);
        }

        [Test]
        public void when_cache_entry_expires_should_notify_eviction()
        {
            var interviewId = Guid.NewGuid();
            var notifier = new Mock<IEvictionNotifier>();
            var cache = new TestableWebTesterAggregateCache(Mock.Of<IMemoryCache>(), notifier.Object);

            cache.NotifyCacheItemRemoved(interviewId, EvictionReason.Expired);

            notifier.Verify(x => x.Evict(interviewId), Times.Once);
        }

        private sealed class TestableWebTesterAggregateCache : WebTesterAggregateCache
        {
            public TestableWebTesterAggregateCache(IMemoryCache memoryCache, IEvictionNotifier notifier)
                : base(memoryCache, notifier)
            {
            }

            public void NotifyCacheItemRemoved(Guid id, EvictionReason reason)
            {
                base.CacheItemRemoved(id, reason);
            }
        }
    }
}
