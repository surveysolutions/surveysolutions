namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The questionnaire status changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireStatusChanged")]
    public class QuestionnaireStatusChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus PreviousStatus { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        #endregion
    }
}