using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireJsonImportService
    {
        /// <summary>
        /// Validates the questionnaire JSON against the questionnaire schema and Survey Solutions domain
        /// rules and, only when it is valid, creates the questionnaire.
        /// </summary>
        QuestionnaireJsonImportResult ValidateAndImport(string questionnaireJson, Guid responsibleId);
    }

    public class QuestionnaireJsonImportResult
    {
        public bool Succeeded { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public string? QuestionnaireTitle { get; set; }
        public string SchemaVersion { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public List<QuestionnaireJsonImportWarning> Warnings { get; set; } = new List<QuestionnaireJsonImportWarning>();
    }

    public class QuestionnaireJsonImportWarning
    {
        public string? Source { get; set; }
        public string Description { get; set; } = string.Empty;

        /// <summary>Imported, Modified or Skipped.</summary>
        public string Action { get; set; } = "Modified";
    }
}
