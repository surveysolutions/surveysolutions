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