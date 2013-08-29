namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The flow graph document.
    /// </summary>
    public class FlowGraphDocument : IFlowGraphDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowGraphDocument"/> class.
        /// </summary>
        public FlowGraphDocument()
        {
            this.CreationDate = DateTime.Now;
            this.LastEntryDate = DateTime.Now;
            this.Blocks = new List<FlowBlock>();
            this.Connections = new List<FlowConnection>();
        }

        #endregion

        // public string Id { get; set; }
        #region Public Properties

        /// <summary>
        /// Gets or sets the blocks.
        /// </summary>
        public List<FlowBlock> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public List<FlowConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire document id.
        /// </summary>
        public Guid QuestionnaireDocumentId { get; set; }

        #endregion
    }
}