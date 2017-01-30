﻿using System;
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
        private readonly ConcurrentDictionary<Type, IEventSourcedAggregateRoot> memoryCache = new ConcurrentDictionary<Type, IEventSourcedAggregateRoot>();

        public EventSourcedAggregateRootRepositoryWithCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
            : base(eventStore, snapshotStore, repository)
        {
        }
        
        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            lock (this.memoryCache)
            {
                var aggregateRoot =
                      this.GetFromCache(aggregateType, aggregateId) ??
                      base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

                if (aggregateRoot != null)
                {
                      this.memoryCache[aggregateType] = aggregateRoot;
                }

                return aggregateRoot; 
            }
        }

        private IEventSourcedAggregateRoot GetFromCache(Type aggregateType, Guid aggregateId)
        {
            IEventSourcedAggregateRoot cachedAggregate;

            bool foundInCache = this.memoryCache.TryGetValue(aggregateType, out cachedAggregate);
            if (!foundInCache) return null;

            bool hasSameId = cachedAggregate.EventSourceId == aggregateId;
            if (!hasSameId) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        public void CleanCache()
        {
            this.memoryCache.Clear();
        }
    }
}