// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireUpdated.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireUpdated")]
    public class QuestionnaireUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [Obsolete]
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}