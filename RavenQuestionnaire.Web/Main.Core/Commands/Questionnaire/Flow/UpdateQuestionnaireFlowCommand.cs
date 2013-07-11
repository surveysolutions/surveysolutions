namespace Main.Core.Commands.Questionnaire.Flow
{
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;

    // check and rewrite
    /// <summary>
    /// The update questionnaire flow command.
    /// </summary>
    public class UpdateQuestionnaireFlowCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateQuestionnaireFlowCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="blocks">
        /// The blocks.
        /// </param>
        /// <param name="connections">
        /// The connections.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        public UpdateQuestionnaireFlowCommand(
            string questionnaireId, List<FlowBlock> blocks, List<FlowConnection> connections, UserLight executor)
        {
            this.QuestionnaireId = questionnaireId;
            this.FlowGraphId = questionnaireId;
            this.Blocks = blocks;
            this.Connections = connections;
            this.Executor = executor;
        }

        #endregion

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
        /// Gets or sets the executor.
        /// </summary>
        public UserLight Executor { get; set; }

        /// <summary>
        /// Gets or sets the flow graph id.
        /// </summary>
        public string FlowGraphId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        #endregion
    }
}