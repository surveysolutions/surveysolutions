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
        private static readonly NamedLocker locker = new NamedLocker();

        public EventSourcedAggregateRootRepositoryWithWebCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
            : base(eventStore, snapshotStore, repository)
        {
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEventSourcedAggregateRoot aggregateRoot = 
                locker.RunWithLock(aggregateId.FormatGuid(),
                        () => this.GetFromCache(aggregateId) ?? base.GetLatest(aggregateType, aggregateId, progress, cancellationToken));

            if (aggregateRoot != null)
            {
                this.PutToTopOfCache(aggregateRoot);
            }

            return aggregateRoot;
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            var cachedAggregate = Cache.Get(aggregateId.ToString()) as IEventSourcedAggregateRoot;

            if (cachedAggregate == null) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        private static Cache Cache => System.Web.HttpContext.Current == null
            ? System.Web.HttpRuntime.Cache
            : System.Web.HttpContext.Current.Cache;

        private void PutToTopOfCache(IEventSourcedAggregateRoot aggregateRoot) => Cache.Insert(aggregateRoot.EventSourceId.ToString(), aggregateRoot, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5));

        public void Evict(Guid aggregateId)
        {
            Cache.Remove(aggregateId.ToString());
        }
    }
}