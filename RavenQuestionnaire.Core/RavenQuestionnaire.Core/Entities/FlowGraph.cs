// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowGraph.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The flow graph.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The flow graph.
    /// </summary>
    public class FlowGraph : IEntity<FlowGraphDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly FlowGraphDocument innerDocument;

        #endregion

        // public string QuestionnaireId { get { return innerDocument; } }
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowGraph"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public FlowGraph(Guid questionnaireId)
        {
            this.innerDocument = new FlowGraphDocument
                {
                   PublicKey = Guid.NewGuid(), QuestionnaireDocumentId = questionnaireId 
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowGraph"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        public FlowGraph(Questionnaire questionnaire)
            : this(questionnaire.PublicKey)
        {
            // Create new from questionnaire
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowGraph"/> class.
        /// </summary>
        /// <param name="innerDocument">
        /// The inner document.
        /// </param>
        public FlowGraph(FlowGraphDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.FlowGraphDocument.
        /// </returns>
        public FlowGraphDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        /// <summary>
        /// The update flow.
        /// </summary>
        /// <param name="blocks">
        /// The blocks.
        /// </param>
        /// <param name="connections">
        /// The connections.
        /// </param>
        public void UpdateFlow(List<FlowBlock> blocks, List<FlowConnection> connections)
        {
            this.innerDocument.Blocks = blocks;
            this.innerDocument.Connections = connections;
            this.innerDocument.LastEntryDate = DateTime.Now;
        }

        #endregion
    }
}