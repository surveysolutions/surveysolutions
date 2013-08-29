namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    /// <summary>
    /// The valid status changed.
    /// </summary>
    [Serializable]
    public class ValidStatusChanged
    {
        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }




    }
}
