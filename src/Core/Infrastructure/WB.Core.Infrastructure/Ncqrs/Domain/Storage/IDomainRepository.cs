using System;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    /// <summary>
    /// A repository that can be used to get and save aggregate roots.
    /// </summary>
    public interface IDomainRepository
    {
        /// <summary>
        /// Recreates an aggregate root using (optionally a snapshot) and a stream of events.
        /// </summary>
        /// <param name="aggreateRootType">Type of aggregate root.</param>
        /// <param name="snapshot">Optional snapshot of state of aggregate root.</param>
        /// <param name="eventStream">A stream of events (is snapshot is provided, stream starts at next event 
        /// after snapshot. Otherwise it starts from the beginning of aggregate's life).</param>
        /// <returns>Aggregate root instance.</returns>
        AggregateRoot Load(Type aggreateRootType, Snapshot snapshot, CommittedEventStream eventStream);
        /// <summary>
        /// Takes a snapshot of provided aggregate root.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate root instance.</param>
        /// <returns>Snapshot instance if aggregate root supports snapthotting. Otherwise null.</returns>
        Snapshot TryTakeSnapshot(IEventSourcedAggregateRoot aggregateRoot);
    }
}