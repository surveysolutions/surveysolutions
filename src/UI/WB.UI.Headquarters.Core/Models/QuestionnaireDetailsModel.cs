using System;
using System.Collections.Generic;

namespace WB.UI.Headquarters.Models
{
    public class QuestionnaireDetailsModel
    {
        public QuestionnaireDetailsModel()
        {
            this.TranslatedPdfVersions = new List<TranslatedPdf>();
        }

        public Guid QuestionnaireId { get; set; }
        public string Title { get; set; }
        public long Version { get; set; }
        public DateTime ImportDateUtc { get; set; }
        public User ImportedBy { get; set; }
        public DateTime LastEntryDateUtc { get; set; }
        public DateTime CreationDateUtc { get; set; }
        public bool AudioAudit { get; set; }
        public bool WebMode { get; set; }
        public int SectionsCount { get; set; }
        public int SubSectionsCount { get; set; }
        public int RostersCount { get; set; }
        public int QuestionsCount { get; set; }
        public int QuestionsWithConditionsCount { get; set; }
        public List<TranslatedPdf> TranslatedPdfVersions { get; set; }
        public string MainPdfUrl { get; set; }
        public string DesignerUrl { get; internal set; }
        public string Comment { get; set; }
        
        public string Variable { get; set; }
        public bool IsObserving { get; set; }

        public string DefaultLanguageName { get; set; }
        public string ExposedVariablesUrl { get; set; }

        public class User
        {
            public string Role { get; set; }
            public string Name { get; set; }
        }
    }

    public struct TranslatedPdf
    {
        public TranslatedPdf(string name, string pdfUrl)
        {
            Name = name;
            PdfUrl = pdfUrl;
        }

        public string Name { get; }

        public string PdfUrl { get; }
    }
}
