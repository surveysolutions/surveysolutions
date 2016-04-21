﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class EventSourcedAggregateRootRepository : IEventSourcedAggregateRootRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        private readonly IDomainRepository repository;

        public EventSourcedAggregateRootRepository(IEventStore eventStore, ISnapshotStore snapshotStore, IDomainRepository repository)
        {
            this.eventStore = eventStore;
            this.snapshotStore = snapshotStore;
            this.repository = repository;
        }

        public virtual IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            Snapshot snapshot = this.snapshotStore.GetSnapshot(aggregateId, int.MaxValue);

            int minVersion = snapshot != null
                ? snapshot.Version + 1
                : 0;

            IEnumerable<CommittedEvent> events = this.eventStore.Read(aggregateId, minVersion);

            return this.repository.Load(aggregateType, aggregateId, snapshot, events);
        }
    }
}