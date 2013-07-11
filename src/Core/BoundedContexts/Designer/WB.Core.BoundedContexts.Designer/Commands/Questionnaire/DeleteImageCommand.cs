namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The delete image command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteImage")]
    public class DeleteImageCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteImageCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        public DeleteImageCommand(Guid questionnaireId, Guid questionKey, Guid imageKey)
        {
            this.QuestionKey = questionKey;
            this.ImageKey = imageKey;
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the image key.
        /// </summary>
        public Guid ImageKey { get; set; }

        /// <summary>
        /// Gets or sets the question key.
        /// </summary>
        public Guid QuestionKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}