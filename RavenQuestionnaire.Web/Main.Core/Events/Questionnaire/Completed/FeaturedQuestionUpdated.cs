namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    /// <summary>
    /// The featured question updated.
    /// </summary>
    [Serializable]
    public class FeaturedQuestionUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

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