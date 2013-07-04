namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The questionnaire assignment changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireAssignmentChanged")]
    public class QuestionnaireAssignmentChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets previous responsible person.
        /// </summary>
        public UserLight PreviousResponsible { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        #endregion
    }
}