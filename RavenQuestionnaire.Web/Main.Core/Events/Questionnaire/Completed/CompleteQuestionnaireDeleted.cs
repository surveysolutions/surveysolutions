namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The complete questionnaire deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:Delete")]
    public class CompleteQuestionnaireDeleted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        #endregion
    }
}