// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessView.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.View.SyncProcess
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;

    /// <summary>
    /// The user view.
    /// </summary>
    public class SyncProcessView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessView"/> class. 
        /// </summary>
        /// <param name="process">
        /// Sync Process Statistics Document
        /// </param>
        public SyncProcessView(SyncProcessStatisticsDocument process)
        {
            this.Messages = new List<UserSyncProcessStatistics>();
            this.Messages.AddRange(process.Statistics);
            this.PublicKey = process.SyncKey;
            this.SyncType = process.SyncType;
            this.IsEnded = process.IsEnded;
            this.CreationDate = process.CreationDate;
            this.EndDate = process.EndDate;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets messages.
        /// </summary>
        public List<UserSyncProcessStatistics> Messages { get; set; }

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
        public SynchronizationType SyncType { get; set; }

        #endregion
    }
}