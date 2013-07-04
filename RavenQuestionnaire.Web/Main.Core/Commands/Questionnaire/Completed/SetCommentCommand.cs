namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The set comment command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetComment")]
    public class SetCommentCommand : CommandBase
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
        /// <param name="questionComments">
        /// The question comments.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        public SetCommentCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            string questionComments,
            Guid? propogationPublicKey,
            UserLight user)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;

            this.User = user;
            this.QuestionPublickey = questionPublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.Comments = questionComments;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

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

        #endregion
    }
}