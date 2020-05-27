using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Metrics;
using WB.Core.Infrastructure.Services;

namespace WB.Infrastructure.Native.Storage
{
    public class EventSourcedAggregateRootRepositoryWithWebCache : EventSourcedAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        private readonly IAggregateRootPrototypeService prototypeService;
        private readonly IServiceLocator serviceLocator;
        private readonly IAggregateLock aggregateLock;
        private readonly IAggregateRootCache memoryCache;
        private readonly IOptions<SchedulerConfig> schedulerOptions;
        private readonly HashSet<Guid> dirtyChecked = new HashSet<Guid>();

        public EventSourcedAggregateRootRepositoryWithWebCache(
            IEventStore eventStore,
            IAggregateRootPrototypeService prototypeService,
            IDomainRepository repository,
            IServiceLocator serviceLocator,
            IAggregateLock aggregateLock,
            IAggregateRootCache memoryCache,
            IOptions<SchedulerConfig> schedulerOptions)
            : base(eventStore, repository)
        {
            this.eventStore = eventStore;
            this.prototypeService = prototypeService;
            this.serviceLocator = serviceLocator;
            this.aggregateLock = aggregateLock;
            this.memoryCache = memoryCache;
            this.schedulerOptions = schedulerOptions;
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId,
            IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregateRootCacheItem = this.GetFromCache(aggregateId);
                var aggregateRoot = aggregateRootCacheItem?.AggregateRoot;

                if (aggregateRoot != null)
                {
                    return aggregateRoot;
                }

                if (this.prototypeService.IsPrototype(aggregateId))
                {
                    IEnumerable<CommittedEvent> events = aggregateRootCacheItem?.Events ?? new List<CommittedEvent>();
                    aggregateRoot = base.repository.Load(aggregateType, aggregateId, events);
                }
                else
                {
                    aggregateRoot = base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);
                }

                if (aggregateRoot != null)
                {
                    this.memoryCache.Set(aggregateRoot);
                }

                return aggregateRoot;
            });
        }

        private AggregateRootCacheItem GetFromCache(Guid aggregateId)
        {
            var cacheItem = memoryCache.Get(aggregateId);

            if (cacheItem == null)
            {
                CoreMetrics.StatefullInterviewCacheMiss?.Inc();
                return null;
            }

            if (cacheItem.AggregateRoot == null)
            {
                return cacheItem;
            }

            bool dbContainsNewEvents = false;

            if (this.schedulerOptions.Value.IsClustered)
            {
                if (!this.prototypeService.IsPrototype(aggregateId))
                {
                    if (!dirtyChecked.Contains(aggregateId))
                    {
                        dbContainsNewEvents = eventStore.IsDirty(aggregateId, cacheItem.AggregateRoot.Version);

                        if (!dbContainsNewEvents)
                        {
                            dirtyChecked.Add(aggregateId);
                        }
                    }
                }
            }

            bool isDirty = cacheItem.AggregateRoot.HasUncommittedChanges() || dbContainsNewEvents;
            if (isDirty)
            {
                this.memoryCache.Evict(aggregateId);
                return null;
            }

            this.serviceLocator.InjectProperties(cacheItem.AggregateRoot);

            CoreMetrics.StatefullInterviewCacheHit?.Inc();
            return cacheItem;
        }
    }
}
