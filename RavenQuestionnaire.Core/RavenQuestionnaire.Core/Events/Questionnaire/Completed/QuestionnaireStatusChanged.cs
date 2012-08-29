// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireStatusChanged.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire status changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The questionnaire status changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireStatusChanged")]
    public class QuestionnaireStatusChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        #endregion
    }
}