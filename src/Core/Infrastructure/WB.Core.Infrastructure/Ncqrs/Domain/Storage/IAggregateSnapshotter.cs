using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public interface IAggregateSnapshotter
    {
        bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, CommittedEventStream committedEventStream, out AggregateRoot aggregateRoot);
        bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, IEnumerable<CommittedEvent> history, out AggregateRoot aggregateRoot);
        bool TryTakeSnapshot(IAggregateRoot aggregateRoot, out Snapshot snapshot);
        void CreateSnapshotIfNeededAndPossible(IAggregateRoot aggregateRoot);
    }
}
