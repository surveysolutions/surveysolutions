namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The create questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CreateQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuestionnaireCommand"/> class.
        /// </summary>
        public CreateQuestionnaireCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="createdBy">
        /// The created by.
        /// </param>
        /// <param name="isPublic">
        /// The is public.
        /// </param>
        public CreateQuestionnaireCommand(Guid questionnaireId, string text, Guid? createdBy = null, bool isPublic = false)
            : base(questionnaireId)
        {
            this.PublicKey = questionnaireId;
            this.Title = text;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the is public.
        /// </summary>
        public bool IsPublic { get; set; }

        #endregion
    }
}