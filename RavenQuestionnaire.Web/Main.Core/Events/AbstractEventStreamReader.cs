// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The abstract event sync.
    /// </summary>
    public abstract class AbstractEventStreamReader : IEventStreamReader
    {
        #region Constants and Fields

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventStreamReader"/> class.
        /// </summary>
        /// <exception cref="Exception">
        /// Exception if IEventStore is not properly initialized
        /// </exception>
        public AbstractEventStreamReader()
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.eventStore == null)
            {
                throw new Exception("IEventStore is not properly initialized.");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read events.
        /// </summary>
        /// <returns>
        /// List of AggregateRootEvent
        /// </returns>
        public abstract IEnumerable<AggregateRootEvent> ReadEvents();

        public abstract IEnumerable<SyncItemsMeta> GetAllARIds();

        public abstract IEnumerable<AggregateRootEvent> GetARById(Guid ARId, string ARType ,Guid? startFrom);

        #endregion
    }
}