// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The sync process document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.View.SyncProcess;

    /// <summary>
    /// The sync process document.
    /// </summary>
    public class SyncProcessDocument : IView
    {
        #region Fields

        /// <summary>
        /// The public key.
        /// </summary>
        private Guid publicKey;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessDocument"/> class.
        /// </summary>
        public SyncProcessDocument()
        {
            this.Chunks = new List<ProcessedEventChunk>();
            this.Statistics = new List<UserSyncProcessStatistics>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the chunks.
        /// </summary>
        public List<ProcessedEventChunk> Chunks { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the handled.
        /// </summary>
        public EventState Handled { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey
        {
            get
            {
                return this.publicKey;
            }

            set
            {
                this.publicKey = value;
                this.Id = this.publicKey.ToString();
            }
        }

        /// <summary>
        /// Gets or sets ParentProcessKey.
        /// </summary>
        public Guid? ParentProcessKey { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the sync type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        /// <summary>
        /// Gets or sets Statistics.
        /// </summary>
        public List<UserSyncProcessStatistics> Statistics { get; set; }

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