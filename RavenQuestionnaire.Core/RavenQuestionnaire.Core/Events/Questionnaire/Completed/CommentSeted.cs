// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommentSeted.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The comment seted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The comment seted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:AnswerSet")]
    public class CommentSeted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question publickey.
        /// </summary>
        public Guid QuestionPublickey { get; set; }

        #endregion
    }
}