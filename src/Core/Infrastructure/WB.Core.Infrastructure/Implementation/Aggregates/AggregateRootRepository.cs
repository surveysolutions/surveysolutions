using System;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class AggregateRootRepository : IAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        private readonly IDomainRepository repository;

        public AggregateRootRepository(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
        {
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
        }

        public virtual IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            Snapshot snapshot = this.snapshotStore.GetSnapshot(aggregateId, int.MaxValue);

            int minVersion = snapshot != null
                ? snapshot.Version + 1
                : int.MinValue;

            CommittedEventStream eventStream = this.eventStore.ReadFrom(aggregateId, minVersion, int.MaxValue);

            return this.repository.Load(aggregateType, snapshot, eventStream);
        }
    }
}