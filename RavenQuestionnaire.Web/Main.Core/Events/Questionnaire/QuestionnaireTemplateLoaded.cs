// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireTemplateLoaded.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire template loaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Documents;

    using Ncqrs.Eventing.Storage;

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