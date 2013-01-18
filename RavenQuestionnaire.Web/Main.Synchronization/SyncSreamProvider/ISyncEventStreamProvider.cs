// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncEventStreamProvider.cs" company="The World Bank">
//   The World Bank
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
        #region Public Properties

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        string ProviderName { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<AggregateRootEvent> GetEventStream();

        /// <summary>
        /// The get total event count.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        int? GetTotalEventCount();

        #endregion
    }
}