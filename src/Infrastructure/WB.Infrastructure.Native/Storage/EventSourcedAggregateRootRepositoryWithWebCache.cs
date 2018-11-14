using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Web.Caching;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Infrastructure.Native.Storage
{
    public class EventSourcedAggregateRootRepositoryWithWebCache : EventSourcedAggregateRootRepository,
        IAggregateRootCacheCleaner, IAggregateRootCacheFiller
    {
        private readonly IInMemoryEventStore inMemoryEventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly IAggregateLock aggregateLock;

        private static readonly ConcurrentDictionary<string, bool> CacheCountTracker = new ConcurrentDictionary<string, bool>();

        public EventSourcedAggregateRootRepositoryWithWebCache(IEventStore eventStore, 
            IInMemoryEventStore inMemoryEventStore,
            EventBusSettings eventBusSettings,
            ISnapshotStore snapshotStore,
            IDomainRepository repository, 
            IAggregateLock aggregateLock)
            : base(eventStore, snapshotStore, repository)
        {
            this.inMemoryEventStore = inMemoryEventStore;
            this.eventBusSettings = eventBusSettings;
            this.aggregateLock = aggregateLock;
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId,
            IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregateRoot = this.GetFromCache(aggregateId);

                if (aggregateRoot == null)
                {
                    if (!this.eventBusSettings.IsIgnoredAggregate(aggregateId))
                    {
                        aggregateRoot = base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);
                    }
                    else
                    {
                        var events = this.inMemoryEventStore.Read(aggregateId, 0);
                        aggregateRoot = base.repository.Load(aggregateType, aggregateId, null, events);
                    }

                    if (aggregateRoot != null)
                        this.PutToCache(aggregateRoot);
                }

                return aggregateRoot;
            });
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            if (!(Cache.Get(Key(aggregateId)) is IEventSourcedAggregateRoot cachedAggregate)) return null;

            bool isDirty = cachedAggregate.HasUncommittedChanges();

            if (isDirty)
            {
                Evict(aggregateId);
                return null;
            }

            return cachedAggregate;
        }

        private static Cache Cache => System.Web.HttpRuntime.Cache;
        protected virtual TimeSpan Expiration => TimeSpan.FromMinutes(5);

        private void PutToCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            var key = Key(aggregateRoot.EventSourceId);

            CacheCountTracker.AddOrUpdate(key, true, (k, old) => true);
            CommonMetrics.StateFullInterviewsCount.Set(CacheCountTracker.Count);

            Cache.Add(key, aggregateRoot, null, Cache.NoAbsoluteExpiration, Expiration, CacheItemPriority.Normal, OnUpdateCallback);
        }

        protected virtual string Key(Guid id) => $"aggregateRoot_" + id.ToString();
        
        private void OnUpdateCallback(string key, object value, CacheItemRemovedReason reason)
        {
            CacheItemRemoved(key);
        }

        protected virtual void CacheItemRemoved(string key)
        {
            CacheCountTracker.TryRemove(key, out _);
            CommonMetrics.StateFullInterviewsCount.Set(CacheCountTracker.Count);
        }

        public void Evict(Guid aggregateId)
        {
            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var key = Key(aggregateId);

                if (Cache.Remove(key) != null)
                {
                    CacheItemRemoved(key);
                }
            });
        }

        public void Store(IEventSourcedAggregateRoot aggregateRoot)
        {
            PutToCache(aggregateRoot);
        }
    }
}
