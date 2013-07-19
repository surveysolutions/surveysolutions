namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;

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