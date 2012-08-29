// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreLoadCompleteQuestionnaireCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the PreLoadCompleteQuestionnaireCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The pre load complete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "PreLoad")]
    public class PreLoadCompleteQuestionnaireCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        #endregion
    }
}