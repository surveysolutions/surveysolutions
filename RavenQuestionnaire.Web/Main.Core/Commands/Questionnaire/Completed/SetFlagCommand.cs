namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The set comment command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetFlag")]
    public class SetFlagCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetCommentCommand"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        /// <param name="isFlaged">
        /// The is Flaged.
        /// </param>
        public SetFlagCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            Guid? propogationPublicKey,
            bool isFlaged)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;

            //// this.Executor = executor;
            this.QuestionPublickey = questionPublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.IsFlaged = isFlaged;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question publickey.
        /// </summary>
        public Guid QuestionPublickey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsFlaged.
        /// </summary>
        public bool IsFlaged { get; set; }

        #endregion


    }
}