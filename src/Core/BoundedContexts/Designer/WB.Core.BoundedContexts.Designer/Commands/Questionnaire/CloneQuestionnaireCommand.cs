namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    ///     The clone questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CloneQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CloneQuestionnaireCommand" /> class.
        /// </summary>
        public CloneQuestionnaireCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloneQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="createdBy">
        /// The created by.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CloneQuestionnaireCommand(Guid publicKey, string title, Guid createdBy, IQuestionnaireDocument doc)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.CreatedBy = createdBy;
            this.Title = title;
            this.Source = doc;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the created by.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        ///     Gets or sets the questionnaire id.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public IQuestionnaireDocument Source { get; set; }

        #endregion
    }
}