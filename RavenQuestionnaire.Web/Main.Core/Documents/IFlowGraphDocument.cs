namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The FlowGraphDocument interface.
    /// </summary>
    public interface IFlowGraphDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the blocks.
        /// </summary>
        List<FlowBlock> Blocks { get; set; }

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        List<FlowConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire document id.
        /// </summary>
        Guid QuestionnaireDocumentId { get; set; }

        #endregion
    }
}