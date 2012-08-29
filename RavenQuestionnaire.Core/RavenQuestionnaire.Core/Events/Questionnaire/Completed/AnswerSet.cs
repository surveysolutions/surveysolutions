// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerSet.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The answer set.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The answer set.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AnswerSet")]
    public class AnswerSet
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer keys.
        /// </summary>
        public List<Guid> AnswerKeys { get; set; }

        /// <summary>
        /// Gets or sets the answer string.
        /// </summary>
        public string AnswerString { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public string AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question public key.
        /// </summary>
        public Guid QuestionPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        #endregion
    }
}