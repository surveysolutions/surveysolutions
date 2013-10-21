using System;
using System.Collections.Generic;
using Main.Core.View.SyncProcess;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Synchronization
{
    /// <summary>
    /// The process ended.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:ProcessStatisticsCalculated")]
    public class ProcessStatisticsCalculated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        public List<UserSyncProcessStatistics> Statistics { get; set; }

        /// <summary>
        /// Gets or sets ProcessKey.
        /// </summary>
        public Guid ProcessKey { get; set; }

        #endregion
    }
}