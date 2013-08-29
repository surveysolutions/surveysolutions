namespace Main.Core.Events.Questionnaire.Completed
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The enable status changed.
    /// </summary>
    [Serializable]
    public class ConditionalStatusChanged
    {
        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// ResultGroupsStatus 
        /// </summary>
        public Dictionary<string, bool?> ResultGroupsStatus { get; set; }

        /// <summary>
        /// ResultQuestionsStatus 
        /// </summary>
        public Dictionary<string, bool?> ResultQuestionsStatus { get; set; }
    }
}
