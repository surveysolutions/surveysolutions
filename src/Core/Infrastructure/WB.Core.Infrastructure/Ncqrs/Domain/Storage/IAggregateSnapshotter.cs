using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public interface IAggregateSnapshotter
    {
        bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, CommittedEventStream committedEventStream, out EventSourcedAggregateRoot aggregateRoot);
        bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, IEnumerable<CommittedEvent> history, out EventSourcedAggregateRoot aggregateRoot);
        bool TryTakeSnapshot(IEventSourcedAggregateRoot aggregateRoot, out Snapshot snapshot);
        void CreateSnapshotIfNeededAndPossible(IEventSourcedAggregateRoot aggregateRoot);
    }
}
