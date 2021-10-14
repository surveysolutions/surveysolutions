using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Questionnaire
    {
        public Questionnaire()
        {
            Id = String.Empty;
            ConditionExpression = String.Empty;
            Title = String.Empty;
            Description = String.Empty;
            VariableName = String.Empty;

            Children = new List<IQuestionnaireEntity>();
            Macros = new Dictionary<Guid, Macro>();
            LookupTables = new Dictionary<Guid, LookupTable>();
            Attachments = new List<Attachment>();
            Translations = new List<Translation>();
            Categories = new List<Categories>();
        }

        public Guid CoverPageSectionId { get; set; }
        public string Id { get; set; }
        public int Revision { get; set; }
        public List<IQuestionnaireEntity> Children { get; set; }
        public Dictionary<Guid, Macro> Macros { get; set; }
        public Dictionary<Guid, LookupTable> LookupTables { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<Translation> Translations { get; set; }
        public List<Categories> Categories { get; set; }
        public Guid? DefaultTranslation { get; set; }
        public DateTime? CloseDate { get; set; }
        public string ConditionExpression { get; set; }
        public bool HideIfDisabled { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public bool IsPublic { get; set; }
        public Guid PublicKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestionnaireMetaInfo? Metadata { get; set; }
        public string VariableName { get; set; }
        public bool IsRoster => false;
        public RosterDisplayMode DisplayMode => RosterDisplayMode.SubSection;
        public Guid? RosterSizeQuestionId => null;
        public RosterSizeSourceType RosterSizeSource => RosterSizeSourceType.Question;
        public string[] RosterFixedTitles { set {} }
        public FixedRosterTitle[] FixedRosterTitles => Array.Empty<FixedRosterTitle>();
        public Guid? RosterTitleQuestionId => null;
        public long LastEventSequence { get; set; }
        public bool IsUsingExpressionStorage { get; set; }
        // fill in before export to HQ or Tester
        public List<Guid>? ExpressionsPlayOrder { get; set; }
        public Dictionary<Guid, Guid[]>? DependencyGraph { get; set; }
        public Dictionary<Guid, Guid[]>? ValidationDependencyGraph { get; set; }
        public string? DefaultLanguageName { get; set; }
    }
}