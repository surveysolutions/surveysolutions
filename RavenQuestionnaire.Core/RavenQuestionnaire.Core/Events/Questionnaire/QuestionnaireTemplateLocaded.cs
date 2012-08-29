// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireTemplateLocaded.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire template loaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The questionnaire template loaded.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireTemplateLoaded")]
    public class QuestionnaireTemplateLoaded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public QuestionnaireDocument Template { get; set; }

        #endregion
    }
}