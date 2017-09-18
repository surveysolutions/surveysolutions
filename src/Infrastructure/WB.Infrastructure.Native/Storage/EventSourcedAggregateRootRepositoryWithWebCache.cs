using System;
using System.Threading;
using System.Web.Caching;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Infrastructure.Native.Storage
{
    public class EventSourcedAggregateRootRepositoryWithWebCache : EventSourcedAggregateRootRepository, IAggregateRootCacheCleaner
    {
        private readonly IAggregateLock aggregateLock;

        public EventSourcedAggregateRootRepositoryWithWebCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository, IAggregateLock aggregateLock)
            : base(eventStore, snapshotStore, repository)
        {
            this.aggregateLock = aggregateLock;
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(),
            () => {
                var aggregateRoot = this.GetFromCache(aggregateId) ??
                    base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);

                if (aggregateRoot != null)
                {
                    this.PutToTopOfCache(aggregateRoot);
                }

                return aggregateRoot;
            });
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            var cachedAggregate = Cache.Get(aggregateId.FormatGuid()) as IEventSourcedAggregateRoot;

            if (cachedAggregate == null) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        private static Cache Cache => System.Web.HttpContext.Current == null
            ? System.Web.HttpRuntime.Cache
            : System.Web.HttpContext.Current.Cache;

        private void PutToTopOfCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            Cache.Insert(aggregateRoot.EventSourceId.FormatGuid(), aggregateRoot, null, Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(5));
        }

        public void Evict(Guid aggregateId)
        {
            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                Cache.Remove(aggregateId.FormatGuid());
            });
        }
    }
}