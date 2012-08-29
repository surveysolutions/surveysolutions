// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreLoadQuestionnaireCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The pre load questionnaire command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The pre load questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "PreLoad")]
    public class PreLoadQuestionnaireCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}