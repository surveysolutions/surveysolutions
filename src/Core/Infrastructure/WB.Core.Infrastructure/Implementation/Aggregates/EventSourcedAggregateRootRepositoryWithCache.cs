using System;
using System.Collections.Concurrent;
using System.Threading;
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
            return GetLatest(aggregateType, aggregateId, null, new CancellationToken());
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEventSourcedAggregateRoot aggregateRoot;

            if (memoryCache.TryGetValue(aggregateType, out aggregateRoot)
                && aggregateRoot.EventSourceId == aggregateId)
                return aggregateRoot;

            aggregateRoot = base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

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