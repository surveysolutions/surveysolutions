// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalStorageStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The local storage stream collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;

    /// <summary>
    /// The local storage stream collector.
    /// </summary>
    public class LocalStorageStreamCollector : ISyncStreamCollector
    {
        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The collect.
        /// </summary>
        /// <param name="chunk">
        /// The chunk.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public bool Collect(IEnumerable<AggregateRootEvent> chunk)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The finish.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Finish()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void PrepareToCollect()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}