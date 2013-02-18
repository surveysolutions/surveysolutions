// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteCollector.cs" company="">
//   
// </copyright>
// <summary>
//   The remote collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidMain.Synchronization
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;
    using Main.Synchronization.SyncStreamCollector;

    /// <summary>
    /// The remote collector.
    /// </summary>
    internal class RemoteCollector : ISyncStreamCollector
    {
        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether support sync stat.
        /// </summary>
        public bool SupportSyncStat
        {
            get
            {
                return false;
            }
        }

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
        /// The finish.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Finish()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get stat.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public List<UserSyncProcessStatistics> GetStat()
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