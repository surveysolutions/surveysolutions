using System;
using System.Collections.Generic;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class EventSourcedAggregateRootRepository : IEventSourcedAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        protected readonly IDomainRepository repository;

        public EventSourcedAggregateRootRepository(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
        {
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
        }

        public virtual IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public virtual IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            Snapshot snapshot = this.snapshotStore.GetSnapshot(aggregateId, int.MaxValue);

            int minVersion = snapshot?.Version + 1 ?? 0;

            IEnumerable<CommittedEvent> events = this.eventStore.Read(aggregateId, minVersion, progress, cancellationToken);

            return this.repository.Load(aggregateType, aggregateId, snapshot, events);
        }

        public virtual IEventSourcedAggregateRoot GetStateless(Type aggregateType, Guid aggregateId)
        {
            int? lastEventSequence = this.eventStore.GetLastEventSequence(aggregateId);
            if (!lastEventSequence.HasValue)
                return null;
            return this.repository.LoadStateless(aggregateType, aggregateId, lastEventSequence.Value);
        }
    }
}
