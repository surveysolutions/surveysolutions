using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class AggregateRootCacheTests
    {
        [Test]
        public void when_call_clear_should_remove_all_cache_entities()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var aggregateRootCache = new AggregateRootCache(memoryCache, Mock.Of<ILogger>());
            aggregateRootCache.CreateEntry(Id.g1, item => item, TimeSpan.FromDays(5));
            aggregateRootCache.CreateEntry(Id.g2, item => item, TimeSpan.FromDays(5));
            aggregateRootCache.CreateEntry(Id.g3, item => item, TimeSpan.FromDays(5));
            
            aggregateRootCache.Clear();

            Assert.That(aggregateRootCache.TryGetValue(Id.g1, out _), Is.False);
            Assert.That(aggregateRootCache.TryGetValue(Id.g2, out _), Is.False);
            Assert.That(aggregateRootCache.TryGetValue(Id.g3, out _), Is.False);
            Assert.That(memoryCache.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void when_call_clear_should_allow_save_new_items()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var aggregateRootCache = new AggregateRootCache(memoryCache, Mock.Of<ILogger>());
            aggregateRootCache.CreateEntry(Id.g1, item => item, TimeSpan.FromDays(5));
            aggregateRootCache.Clear();
            
            aggregateRootCache.CreateEntry(Id.g2, item => item, TimeSpan.FromDays(5));

            Assert.That(aggregateRootCache.TryGetValue(Id.g1, out _), Is.False);
            Assert.That(aggregateRootCache.TryGetValue(Id.g2, out _), Is.True);
            Assert.That(memoryCache.Count, Is.EqualTo(1));
        }

    }
}
