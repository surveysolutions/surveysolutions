namespace DataEntryClient.SycProcessRepository
{
    using System.Collections.Generic;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;

    using Ncqrs.Eventing;

    /// <summary>
    /// Sync Procesor Interface
    /// </summary>
    public interface ISyncProcessor
    {
        /// <summary>
        /// Gets or sets IncomeEvents.
        /// </summary>
        UncommittedEventStream[] IncomeEvents { get; set; }

        /// <summary>
        /// Calculate statistics
        /// </summary>
        /// <returns>
        /// List of UserSyncProcessStatistics
        /// </returns>
        List<UserSyncProcessStatistics> CalculateStatistics();

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
    }
}