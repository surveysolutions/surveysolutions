// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetCommentCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the SetCommentCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Views.Question;

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
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        public SetCommentCommand(
            Guid completeQuestionnaireId, CompleteQuestionView question, Guid? propogationPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;

            //// this.Executor = executor;
            this.QuestionPublickey = question.PublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.Comments = question.Comments;
        }

        #endregion

        #region Public Properties

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