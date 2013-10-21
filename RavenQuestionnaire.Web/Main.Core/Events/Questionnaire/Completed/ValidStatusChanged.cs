using System;

namespace Main.Core.Events.Questionnaire.Completed
{
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
