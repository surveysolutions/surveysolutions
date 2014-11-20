using System;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.Aggregates
{
    // TODO: TLK, KP-4337: this should be moved to WB.Core.Infrastructure.Aggregates when NCQRS will be portable
    public class AggregateRootRepository : IAggregateRootRepository
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

        public IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            Snapshot snapshot = this.snapshotStore.GetSnapshot(aggregateId, long.MaxValue);

            long minVersion = snapshot != null
                ? snapshot.Version + 1
                : long.MinValue;

            CommittedEventStream eventStream = this.eventStore.ReadFrom(aggregateId, minVersion, long.MaxValue);

            return this.repository.Load(aggregateType, snapshot, eventStream);
        }
    }
}