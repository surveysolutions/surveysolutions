// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeStatusCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   COmmand is used for status changing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// COmmand is used for status changing.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "ChangeStatus")]
    public class ChangeStatusCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        #endregion
    }
}