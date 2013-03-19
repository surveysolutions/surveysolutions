// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteQuestionCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The delete question command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The delete question command.
    /// </summary>
    [Obsolete]
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteQuestion")]
    public class DeleteQuestionCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteQuestionCommand"/> class.
        /// </summary>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public DeleteQuestionCommand(Guid questionId, Guid parentPublicKey, Guid questionnaireId)
        {
            this.QuestionId = questionId;
            this.QuestionnaireId = questionnaireId;
            this.ParentPublicKey = parentPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the question id.
        /// </summary>
        public Guid QuestionId { get; private set; }

        /// <summary>
        /// Gets the parent public key.
        /// </summary>
        public Guid ParentPublicKey { get; private set; }


        /// <summary>
        /// Gets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }

        #endregion
    }
}