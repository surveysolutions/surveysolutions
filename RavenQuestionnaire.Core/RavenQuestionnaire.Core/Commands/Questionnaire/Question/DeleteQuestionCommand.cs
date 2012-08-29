// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteQuestionCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The delete question command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The delete question command.
    /// </summary>
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
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public DeleteQuestionCommand(Guid questionId, Guid questionnaireId)
        {
            this.QuestionId = questionId;
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}