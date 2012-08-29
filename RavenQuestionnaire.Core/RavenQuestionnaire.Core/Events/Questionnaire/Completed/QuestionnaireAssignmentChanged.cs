// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireAssignmentChanged.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire assignment changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The questionnaire assignment changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireAssignmentChanged")]
    public class QuestionnaireAssignmentChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        #endregion
    }
}