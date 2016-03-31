using System;
using System.Collections.Concurrent;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class EventSourcedAggregateRootRepositoryWithCache : EventSourcedAggregateRootRepository, IEventSourcedAggregateRootRepositoryWithCache
    {
        static readonly ConcurrentDictionary<Type, IEventSourcedAggregateRoot> memoryCache = new ConcurrentDictionary<Type, IEventSourcedAggregateRoot>();

        public EventSourcedAggregateRootRepositoryWithCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
            : base(eventStore, snapshotStore, repository)
        {
        }


        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            IEventSourcedAggregateRoot aggregateRoot;

            if (memoryCache.TryGetValue(aggregateType, out aggregateRoot)
                && aggregateRoot.EventSourceId == aggregateId)
                return aggregateRoot;

            aggregateRoot = base.GetLatest(aggregateType, aggregateId);

            if (aggregateRoot != null)
            {
                memoryCache[aggregateType] = aggregateRoot;
            }

            return aggregateRoot;
        }

        public void CleanCache()
        {
            memoryCache.Clear();
        }
    }
}