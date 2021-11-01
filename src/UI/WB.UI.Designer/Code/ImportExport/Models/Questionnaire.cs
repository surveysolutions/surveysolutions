#nullable enable

using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Questionnaire
    {
        public Questionnaire()
        {
            Description = String.Empty;
            VariableName = String.Empty;

            Children = new List<Group>();
            Macros = new Dictionary<Guid, Macro>();
            LookupTables = new Dictionary<Guid, LookupTable>();
            Attachments = new List<Attachment>();
            Translations = new List<Translation>();
            Categories = new List<Categories>();
        }

        public Guid Id { get; set; }

        public CoverPage? CoverPage { get; set; }
        public List<Group> Children { get; set; }
        public Dictionary<Guid, Macro> Macros { get; set; }
        public Dictionary<Guid, LookupTable> LookupTables { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<Translation> Translations { get; set; }
        public List<Categories> Categories { get; set; }
        public Guid? DefaultTranslation { get; set; }
        public bool HideIfDisabled { get; set; }
        public string? Title { get; set; }
        public string Description { get; set; }
        public QuestionnaireMetaInfo? Metadata { get; set; }
        public string VariableName { get; set; }
    }
}