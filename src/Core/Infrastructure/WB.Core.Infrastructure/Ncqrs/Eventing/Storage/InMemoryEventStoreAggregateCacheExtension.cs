#nullable enable
using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Eventing.Storage
{
    public static class InMemoryEventStoreAggregateCacheExtension
    {
        private const string Key = "events";

        public static IEnumerable<CommittedEvent>? GetEvents(this AggregateRootCacheItem? item)
        {
            return item?.Meta.GetValueOrDefault(Key) as IEnumerable<CommittedEvent>;
        }

        public static void SetEvents(this AggregateRootCacheItem item, IEnumerable<CommittedEvent> events)
        {
            item.Meta[Key] = events;
        }

        public static IEnumerable<CommittedEvent>? GetEvents(this IAggregateRootCache cache, Guid id)
        {
            return cache.Get(id).GetEvents();
        }
    }
}
