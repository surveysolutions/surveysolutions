using System;
using System.Collections.Concurrent;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class EventSourcedAggregateRootRepositoryWithCache : EventSourcedAggregateRootRepository, IEventSourcedAggregateRootRepositoryWithCache
    {
        private readonly IAggregateLock aggregateLock;
        private static readonly ConcurrentDictionary<Type, IEventSourcedAggregateRoot> MemoryCache 
            = new ConcurrentDictionary<Type, IEventSourcedAggregateRoot>();

        public EventSourcedAggregateRootRepositoryWithCache(IEventStore eventStore, ISnapshotStore snapshotStore, 
            IDomainRepository repository, IAggregateLock aggregateLock)
            : base(eventStore, snapshotStore, repository)
        {
            this.aggregateLock = aggregateLock;
        }
        
        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregateRoot =
                    this.GetFromCache(aggregateType, aggregateId) ??
                    base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

                if (aggregateRoot != null)
                {
                    MemoryCache[aggregateType] = aggregateRoot;
                }

                return aggregateRoot;
            });
        }

        private IEventSourcedAggregateRoot GetFromCache(Type aggregateType, Guid aggregateId)
        {
            bool foundInCache = MemoryCache.TryGetValue(aggregateType, out var cachedAggregate);
            if (!foundInCache)
                return null;

            bool hasSameId = cachedAggregate.EventSourceId == aggregateId;
            if (!hasSameId)
                return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty)
                return null;

            return cachedAggregate;
        }

        public void CleanCache()
        {
            MemoryCache.Clear();
        }
    }
}
