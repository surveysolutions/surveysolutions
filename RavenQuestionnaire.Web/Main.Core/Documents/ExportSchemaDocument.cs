namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The export schema document.
    /// </summary>
    public class ExportSchemaDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire mapping.
        /// </summary>
        public Dictionary<Guid, string> QuestionnaireMapping { get; set; }

        #endregion
    }
}