// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessStatisticsDocument.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.SyncProcess;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Collect user statistics
    /// </summary>
    public class SyncProcessStatisticsDocument : IView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessStatisticsDocument"/> class.
        /// </summary>
        /// <param name="syncKey">
        /// The sync Key.
        /// </param>
        public SyncProcessStatisticsDocument(Guid syncKey)
        {
            this.PublicKey = syncKey;
            this.CreationDate = DateTime.Now;
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
        /// Gets or sets ParentProcessKey.
        /// </summary>
        public Guid? ParentProcessKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public SynchronizationType SyncType { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets ExitDescription.
        /// </summary>
        public string ExitDescription { get; set; }

        #endregion
    }
}