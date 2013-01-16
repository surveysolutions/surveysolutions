// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncEventStreamProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The SyncEventStreamProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncSreamProvider
{
    using System.Collections.Generic;

    using Main.Core.Events;

    /// <summary>
    /// The SyncEventStreamProvider interface.
    /// </summary>
    public interface ISyncEventStreamProvider
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<AggregateRootEvent> GetEventStream();

        #endregion
    }
}