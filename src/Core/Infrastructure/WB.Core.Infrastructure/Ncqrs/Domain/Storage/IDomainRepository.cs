using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
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
        /// <param name="aggregateRootType">Type of aggregate root.</param>
        /// <param name="snapshot">Optional snapshot of state of aggregate root.</param>
        /// <param name="eventStream">A stream of events (is snapshot is provided, stream starts at next event 
        /// after snapshot. Otherwise it starts from the beginning of aggregate's life).</param>
        /// <returns>Aggregate root instance.</returns>
        IEventSourcedAggregateRoot Load(Type aggregateRootType, CommittedEventStream eventStream);

        IEventSourcedAggregateRoot Load(Type aggregateRootType, Guid aggregateRootId, IEnumerable<CommittedEvent> events);

        IEventSourcedAggregateRoot LoadStateless(Type aggregateRootType, Guid aggregateRootId, int lastEventSequence);
    }
}
