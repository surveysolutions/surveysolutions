// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcesorStub.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClientTests.Stubs
{
    using System.Collections.Generic;

    using DataEntryClient.SycProcess;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;

    using Ncqrs.Eventing;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessorStub : ISyncProcessor
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessorStub"/> class.
        /// </summary>
        public SyncProcessorStub()
        {
            this.IncomeEvents = new UncommittedEventStream[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets IncomeEvents.
        /// </summary>
        public UncommittedEventStream[] IncomeEvents { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The calculate statistics.
        /// </summary>
        /// <returns>
        /// List of stat items
        /// </returns>
        public List<UserSyncProcessStatistics> CalculateStatistics()
        {
            return new List<UserSyncProcessStatistics>();
        }

        /// <summary>
        /// The commit.
        /// </summary>
        public void Commit()
        {
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void Merge(IEnumerable<AggregateRootEvent> stream)
        {
        }

        #endregion
    }
}