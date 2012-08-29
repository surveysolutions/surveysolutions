// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteCompleteQuestionnaireCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The delete complete questionnaire command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The delete complete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "Delete")]
    public class DeleteCompleteQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCompleteQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        public DeleteCompleteQuestionnaireCommand(Guid completeQuestionnaireId)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        #endregion
    }
}