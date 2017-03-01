using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class EventSourcedAggregateRootRepositoryWithExtendedCache : EventSourcedAggregateRootRepository, IAggregateRootCacheCleaner
    {
        private readonly int cacheSize;

        private static readonly object lockObject = new object();

        private ImmutableList<IEventSourcedAggregateRoot> cache = ImmutableList<IEventSourcedAggregateRoot>.Empty;

        public EventSourcedAggregateRootRepositoryWithExtendedCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository, int cacheSize = 100)
            : base(eventStore, snapshotStore, repository)
        {
            this.cacheSize = cacheSize;
        }
        
        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEventSourcedAggregateRoot aggregateRoot
                = this.GetFromCache(aggregateId)
                ?? base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

            if (aggregateRoot != null)
            {
                this.PutToTopOfCache(aggregateRoot);
            }

            return aggregateRoot;
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            var cachedAggregate = this.cache.SingleOrDefault(aggregate => aggregate.EventSourceId == aggregateId);

            if (cachedAggregate == null) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        private void PutToTopOfCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            lock (lockObject)
            {
                if (this.cache.FirstOrDefault() == aggregateRoot) return;

                this.cache = ImmutableList.CreateRange(
                    Enumerable
                        .Concat(
                            aggregateRoot.ToEnumerable(),
                            cache.Except(aggregate => aggregate.EventSourceId == aggregateRoot.EventSourceId))
                        .Take(cacheSize));
            }
        }

        public void Evict(Guid aggregateId)
        {
            lock (lockObject)
            {
                this.cache = ImmutableList.CreateRange(cache.Except(aggregate => aggregate.EventSourceId == aggregateId));
            }
        }
    }
}