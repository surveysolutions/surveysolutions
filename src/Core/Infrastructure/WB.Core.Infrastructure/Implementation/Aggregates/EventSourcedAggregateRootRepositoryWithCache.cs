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
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            lock (memoryCache)
            {
                var aggregateRoot =
                      GetFromCache(aggregateType, aggregateId) ??
                      base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

                if (aggregateRoot != null)
                {
                    memoryCache[aggregateType] = aggregateRoot;
                }

                return aggregateRoot; 
            }
        }

        private static IEventSourcedAggregateRoot GetFromCache(Type aggregateType, Guid aggregateId)
        {
            IEventSourcedAggregateRoot cachedAggregate;

            bool foundInCache = memoryCache.TryGetValue(aggregateType, out cachedAggregate);
            if (!foundInCache) return null;

            bool hasSameId = cachedAggregate.EventSourceId == aggregateId;
            if (!hasSameId) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        public void CleanCache()
        {
            memoryCache.Clear();
        }
    }
}