// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncProcessor.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   Sync Procesor Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SycProcessRepository
{
    using System.Collections.Generic;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;

    using Ncqrs.Eventing;

    /// <summary>
    /// Sync Processor Interface
    /// </summary>
    public interface ISyncProcessor
    {
        #region Public Methods and Operators

        /// <summary>
        /// Calculate statistics
        /// </summary>
        /// <returns>
        /// List of UserSyncProcessStatistics
        /// </returns>
        void PostProcess();

        /// <summary>
        /// The commit.
        /// </summary>
        void Commit();

        /// <summary>
        /// The merge events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void Merge(IEnumerable<AggregateRootEvent> stream);

        #endregion
    }
}