// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCompleteQuestionnaireCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the CreateCompleteQuestionnaireCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The create complete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "CreateCompletedQ")]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCompleteQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="completedQuestionnaireId">
        /// The completed questionnaire id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, Guid questionnaireId)
        {
            this.CompleteQuestionnaireId = completedQuestionnaireId;
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}