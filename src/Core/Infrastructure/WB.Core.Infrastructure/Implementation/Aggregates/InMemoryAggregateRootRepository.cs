using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class InMemoryAggregateRootRepository : IAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        private readonly IDomainRepository repository;

        readonly Dictionary<Type, WeakReference<IAggregateRoot>> memoryCache = new Dictionary<Type, WeakReference<IAggregateRoot>>(); 

        public InMemoryAggregateRootRepository(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
        {
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
        }

        public IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            WeakReference<IAggregateRoot> aggregateRootWeakRef;
            IAggregateRoot aggregateRoot;

            if (memoryCache.TryGetValue(aggregateType, out aggregateRootWeakRef)
                && aggregateRootWeakRef.TryGetTarget(out aggregateRoot)
                && aggregateRoot.EventSourceId == aggregateId)
                return aggregateRoot;

            Snapshot snapshot = this.snapshotStore.GetSnapshot(aggregateId, long.MaxValue);

            long minVersion = snapshot != null
                ? snapshot.Version + 1
                : long.MinValue;

            CommittedEventStream eventStream = this.eventStore.ReadFrom(aggregateId, minVersion, long.MaxValue);

            var root = this.repository.Load(aggregateType, snapshot, eventStream);
            memoryCache.Add(aggregateType, new WeakReference<IAggregateRoot>(root));
            return root;
        }
    }
}