using System;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// An event store. Can store and load events from an <see cref="IEventSource"/>.
    /// </summary>
    public interface IEventStore
    {
        /// <remarks>
        /// Returned event stream does not contain snapshots. This method is used when snapshots are stored in a separate store.
        /// </remarks>
        /// <param name="id">The id of the event source that owns the events.</param>
        /// <param name="minVersion">The minimum version number to be read.</param>
        /// <param name="maxVersion">The maximum version number to be read</param>
        /// <returns>All the events from the event source between specified version numbers.</returns>
        CommittedEventStream ReadFrom(Guid id, int minVersion, int maxVersion);

        /// <summary>
        /// Persists the <paramref name="eventStream"/> in the store as a single and atomic commit.
        /// </summary>
        /// <param name="eventStream">The <see cref="UncommittedEventStream"/> to commit.</param>
        void Store(UncommittedEventStream eventStream);
    }
}