// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewCompleteQuestionnaireCreated.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The new complete questionnaire created.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The new complete questionnaire created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewCompleteQuestionnaireCreated")]
    public class NewCompleteQuestionnaireCreated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire.
        /// </summary>
        public CompleteQuestionnaireDocument Questionnaire { get; set; }

        /// <summary>
        /// Gets or sets the total question count.
        /// </summary>
        public int TotalQuestionCount { get; set; }

        #endregion
    }
}