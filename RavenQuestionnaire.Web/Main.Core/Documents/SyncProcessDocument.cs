// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The sync process document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The sync process document.
    /// </summary>
    public class SyncProcessDocument
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
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the synck type.
        /// </summary>
        public SynchronizationType SynckType { get; set; }

        #endregion
    }
}