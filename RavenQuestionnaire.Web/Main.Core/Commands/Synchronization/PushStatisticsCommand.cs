namespace Main.Core.Commands.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Events;
    using Main.Core.View.SyncProcess;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The push events command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(SyncProcessAR), "PushStatistics")]
    public class PushStatisticsCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PushStatisticsCommand"/> class.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <param name="statistics">
        /// The statistics.
        /// </param>
        public PushStatisticsCommand(Guid syncKey, List<UserSyncProcessStatistics> statistics)
        {
            this.Statistics = statistics;
            this.ProcessGuid = syncKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the event chuncks.
        /// </summary>
        public IList<UserSyncProcessStatistics> Statistics { get; set; }

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        [AggregateRootId]
        public Guid ProcessGuid { get; set; }

        #endregion
    }
}