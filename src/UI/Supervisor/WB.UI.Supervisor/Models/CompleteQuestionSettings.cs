namespace Web.Supervisor.Models
{
    using System;

    /// <summary>
    /// Define complete question settings
    /// </summary>
    public class CompleteQuestionSettings
    {
        public Guid QuestionnaireId { get; set; }
        /// Gets or sets QuestionnaireId.
        /// </summary>

        /// <summary>
        /// Gets or sets ParentGroupPublicKey.
        /// </summary>
        public Guid ParentGroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets PropogationPublicKey.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }
    }
}