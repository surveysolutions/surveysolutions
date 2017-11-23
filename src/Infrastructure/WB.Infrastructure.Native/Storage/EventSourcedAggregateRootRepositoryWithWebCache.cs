using System;
using System.Threading;
using System.Web.Caching;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using Prometheus;
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
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregateRoot = this.GetFromCache(aggregateId);

                if(aggregateRoot == null) {

                    aggregateRoot = base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);
                    if (aggregateRoot != null)
                        this.PutToCache(aggregateRoot);
                }

                return aggregateRoot;
            });
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            if (!(Cache.Get(aggregateId.FormatGuid()) is IEventSourcedAggregateRoot cachedAggregate)) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();
            if (isDirty) return null;

            return cachedAggregate;
        }

        private static Cache Cache => System.Web.HttpRuntime.Cache;

        private static readonly Gauge StatefullInterviewCacheCounter = Metrics.CreateGauge(
            "wb_hq_cache_statefull_interview_counter",
            "Number of statefull interviews stored in HttpRuntime.Cache");

        private void PutToCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            StatefullInterviewCacheCounter.Inc();
            
            Cache.Insert(aggregateRoot.EventSourceId.FormatGuid(), aggregateRoot, null, Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(5), OnUpdateCallback);
        }

        private void OnUpdateCallback(string key, CacheItemUpdateReason reason, 
            out object expensiveObject, 
            out CacheDependency dependency, 
            out DateTime absoluteExpiration, 
            out TimeSpan slidingExpiration)
        {
            expensiveObject = null; dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;

            StatefullInterviewCacheCounter.Dec();
        }

        public void Evict(Guid aggregateId)
        {
            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                StatefullInterviewCacheCounter.Dec();
                Cache.Remove(aggregateId.FormatGuid());
            });
        }
    }
}