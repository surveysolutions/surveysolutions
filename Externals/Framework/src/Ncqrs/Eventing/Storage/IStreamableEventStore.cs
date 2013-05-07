using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    /// <summary>
    /// The StreamableEventStore interface.
    /// </summary>
    public interface IStreamableEventStore : IEventStore
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<CommittedEvent> GetEventStream();

        #warning move to readlayer
        CommittedEvent GetLastEvent(Guid aggregateRootId);
        #warning move to readlayer
        bool IsEventPresent(Guid aggregateRootId, Guid eventIdentifier);

        #endregion
    }
}
