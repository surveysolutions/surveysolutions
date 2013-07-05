namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The update image command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateImage")]
    public class UpdateImageCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateImageCommand"/> class.
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
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="desc">
        /// The desc.
        /// </param>
        public UpdateImageCommand(Guid questionnaireId, Guid questionKey, Guid imageKey, string title, string desc)
        {
            this.QuestionKey = questionKey;
            this.ImageKey = imageKey;
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
            this.Description = desc;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

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

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}