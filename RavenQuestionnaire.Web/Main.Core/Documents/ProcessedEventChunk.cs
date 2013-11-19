using System;
using System.Collections.Generic;

namespace Main.Core.Documents
{
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

        #endregion
    }
}