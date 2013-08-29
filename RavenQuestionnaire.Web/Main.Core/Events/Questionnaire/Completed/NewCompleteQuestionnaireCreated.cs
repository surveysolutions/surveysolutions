namespace Main.Core.Events.Questionnaire.Completed
{
    using System;
    using Main.Core.Documents;
    using Ncqrs.Eventing.Storage;

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