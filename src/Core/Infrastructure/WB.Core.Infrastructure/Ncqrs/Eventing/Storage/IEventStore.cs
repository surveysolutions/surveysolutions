using System;
using System.Collections.Generic;
using System.Threading;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// An event store. Can store and load events from an <see cref="IEventSource"/>.
    /// </summary>
    public interface IEventStore
    {
        IEnumerable<CommittedEvent> Read(Guid id, int minVersion);
        IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken);
        int? GetLastEventSequence(Guid id);
        /// <summary>
        /// Persists the <paramref name="eventStream"/> in the store as a single and atomic commit.
        /// </summary>
        /// <param name="eventStream">The <see cref="UncommittedEventStream"/> to commit.</param>
        CommittedEventStream Store(UncommittedEventStream eventStream);
    }

    public interface IInMemoryEventStore : IEventStore
    {
    }
}
