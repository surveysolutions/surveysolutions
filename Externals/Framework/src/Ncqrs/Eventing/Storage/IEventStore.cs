﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// An event store. Can store and load events from an <see cref="IEventSource"/>.
    /// </summary>
    [ContractClass(typeof(IEventStoreContracts))]
    public interface IEventStore
    {
        /// <summary>
        /// Reads from the stream from the <paramref name="minVersion"/> up until <paramref name="maxVersion"/>.
        /// </summary>
        /// <remarks>
        /// Returned event stream does not contain snapshots. This method is used when snapshots are stored in a separate store.
        /// </remarks>
        /// <param name="id">The id of the event source that owns the events.</param>
        /// <param name="minVersion">The minimum version number to be read.</param>
        /// <param name="maxVersion">The maximum version number to be read</param>
        /// <returns>All the events from the event source between specified version numbers.</returns>
        CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion);

        /// <summary>
        /// Persists the <paramref name="eventStream"/> in the store as a single and atomic commit.
        /// </summary>
        /// <exception cref="ConcurrencyException">Occurs when there is already a newer version of the event provider stored in the event store.</exception>
        /// <param name="eventStream">The <see cref="UncommittedEventStream"/> to commit.</param>
        void Store(UncommittedEventStream eventStream);
        /// <summary>
        /// Reads from the stream from the <paramref name="start"/>
        /// </summary>
        /// <remarks>
        /// Returned event stream of all events created after <paramref name="start"/>
        /// </remarks>
        /// <param name="start">Start date</param>
        /// <returns>All the events from the event source created after <paramref name="start"/></returns>
        IEnumerable<CommittedEvent> ReadFrom(DateTime start);
    }

    [ContractClassFor(typeof(IEventStore))]
    internal abstract class IEventStoreContracts : IEventStore
    {
        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            Contract.Ensures(Contract.Result<CommittedEventStream>().SourceId == id);
            Contract.Ensures(Contract.Result<CommittedEventStream>().CurrentSourceVersion <= maxVersion);
            return default(CommittedEventStream);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            Contract.Requires<ArgumentNullException>(eventStream != null, "The stream cannot be null.");
        }

        public IEnumerable<CommittedEvent> ReadFrom(DateTime start)
        {
            return default(CommittedEventStream);
        }

    }
}