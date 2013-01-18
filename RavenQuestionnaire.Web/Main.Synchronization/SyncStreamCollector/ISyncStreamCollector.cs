// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The SyncStreamCollector interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;

    /// <summary>
    /// The SyncStreamCollector interface.
    /// </summary>
    public interface ISyncStreamCollector : IDisposable
    {
        #region Public Methods and Operators

        /// <summary>
        /// The collect.
        /// </summary>
        /// <param name="chunk">
        /// The chunk.
        /// </param>
        bool Collect(IEnumerable<AggregateRootEvent> chunk);

        /// <summary>
        /// The finish.
        /// </summary>
        void Finish();

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        void PrepareToCollect();

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        int MaxChunkSize { get; }

        #endregion
    }
}