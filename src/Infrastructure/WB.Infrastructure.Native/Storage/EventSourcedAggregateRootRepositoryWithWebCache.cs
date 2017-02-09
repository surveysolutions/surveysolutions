using System;
using System.Threading;
using System.Web.Caching;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;

namespace WB.Infrastructure.Native.Storage
{
    public class EventSourcedAggregateRootRepositoryWithWebCache : EventSourcedAggregateRootRepository
    {
        public EventSourcedAggregateRootRepositoryWithWebCache(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
            : base(eventStore, snapshotStore, repository)
        {
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
            var cachedAggregate = System.Web.HttpContext.Current.Cache.Get(aggregateId.ToString()) as IEventSourcedAggregateRoot;

            if (cachedAggregate == null) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        private void PutToTopOfCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            System.Web.HttpContext.Current.Cache.Insert(aggregateRoot.EventSourceId.ToString(), aggregateRoot,null,Cache .NoAbsoluteExpiration, TimeSpan.FromMinutes(5));
        }
    }
}