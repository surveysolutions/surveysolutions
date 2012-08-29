// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The sync process document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Events;

    /// <summary>
    /// The event state.
    /// </summary>
    public enum EventState
    {
        /// <summary>
        /// The initial.
        /// </summary>
        Initial, 

        /// <summary>
        /// The in progress.
        /// </summary>
        InProgress, 

        /// <summary>
        /// The completed.
        /// </summary>
        Completed, 

        /// <summary>
        /// The error.
        /// </summary>
        Error
    }

    /// <summary>
    /// The synchronization type.
    /// </summary>
    public enum SynchronizationType
    {
        /// <summary>
        /// The push.
        /// </summary>
        Push, 

        /// <summary>
        /// The pull.
        /// </summary>
        Pull
    }

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

    /// <summary>
    /// The processed event chunk.
    /// </summary>
    public class ProcessedEventChunk
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessedEventChunk"/> class.
        /// </summary>
        public ProcessedEventChunk()
        {
            this.EventChunckPublicKey = Guid.NewGuid();
            this.EventKeys = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessedEventChunk"/> class.
        /// </summary>
        /// <param name="eventDoc">
        /// The event doc.
        /// </param>
        public ProcessedEventChunk(IEnumerable<AggregateRootEvent> eventDoc)
            : this()
        {
            this.EventKeys = eventDoc.Select(e => e.EventIdentifier).ToList();
            this.Handled = EventState.Initial;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the event chunck public key.
        /// </summary>
        public Guid EventChunckPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the event keys.
        /// </summary>
        public List<Guid> EventKeys { get; set; }

        /// <summary>
        /// Gets or sets the handled.
        /// </summary>
        public EventState Handled { get; set; }

        #endregion
    }
}