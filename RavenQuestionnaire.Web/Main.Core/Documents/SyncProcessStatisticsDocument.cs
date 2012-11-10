// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessStatisticsDocument.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.SyncProcess;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Collect user statistics
    /// </summary>
    public class SyncProcessStatisticsDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessStatisticsDocument"/> class.
        /// </summary>
        public SyncProcessStatisticsDocument()
        {
            this.CreationDate = DateTime.Now;
            this.PublicKey = Guid.Empty;
            this.IsEnded = false;
            this.Statistics = new List<UserSyncProcessStatistics>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Assignments.
        /// </summary>
        public List<UserSyncProcessStatistics> Statistics { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsEnded.
        /// </summary>
        public bool IsEnded { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid SyncKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public SynchronizationType SyncType { get; set; }

        #endregion
    }
}