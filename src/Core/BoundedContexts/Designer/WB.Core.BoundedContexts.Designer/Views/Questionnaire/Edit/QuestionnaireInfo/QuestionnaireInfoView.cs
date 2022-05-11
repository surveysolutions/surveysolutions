using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoView
    {
        public QuestionnaireInfoView( List<CountryItem> countries,
            string? defaultLanguageName, 
            string questionnaireId, string questionnaireRevision, 
            bool hideIfDisabled,
            bool isPublic,
            bool isCoverPageSupported,
            string? title = null, 
            string? variable = null)
        {
            this.SharedPersons = new List<SharedPersonView>();
            this.Chapters = new List<ChapterInfoView>();
            this.Macros = new List<MacroView>();
            this.LookupTables = new List<LookupTableView>();
            this.Attachments = new List<AttachmentView>();
            this.Translations = new List<TranslationView>();


            Metadata = new MetadataView();
            StudyTypes = new List<StudyTypeItem>();
            KindsOfData = new List<KindOfDataItem>();
            
            ModesOfDataCollection = new List<ModeOfDataCollectionItem>();
            Scenarios = new List<ScenarioView>();
            Categories = new List<CategoriesView>();
            DefaultLanguageName = defaultLanguageName;
            QuestionnaireId = questionnaireId;
            QuestionnaireRevision = questionnaireRevision;
            Title = title;
            Variable = variable;
            HideIfDisabled = hideIfDisabled;
            IsPublic = isPublic;
            Countries = countries;
            IsCoverPageSupported = isCoverPageSupported;
        }

        public string QuestionnaireId { get; set; }
        public string QuestionnaireRevision { get; set; }
        public string? Title { get; set; }
        public string? Variable { get; set; }
        public bool IsPublic { get; set; }
        public bool WebTestAvailable { get; set; }
        public List<ChapterInfoView> Chapters { get; set; }
        public List<MacroView> Macros { get; set; }
        public List<LookupTableView> LookupTables { get; set; }

        public List<SharedPersonView> SharedPersons { get; set; }
        public int QuestionsCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }

        public bool IsReadOnlyForUser { get; set; }
        public bool IsSharedWithUser { get; set; }

        public bool HasViewerAdminRights { get; set; } 

        public List<AttachmentView> Attachments { get; set; }

        public List<TranslationView> Translations { get; set; }

        public MetadataView Metadata { get; set; }

        public List<StudyTypeItem> StudyTypes { get; set; }
        public List<KindOfDataItem> KindsOfData { get; set; }
        public List<CountryItem> Countries { get; set; }
        public List<ModeOfDataCollectionItem> ModesOfDataCollection { get; set; }
        public bool HideIfDisabled { get; set; }

        public List<ScenarioView> Scenarios { get; set; }
        public List<CategoriesView> Categories { get; set; }
        
        public int? PreviewRevision { get; set; }
        public string? DefaultLanguageName { get; set; }

        public bool IsCoverPageSupported { get; set; }
        public bool IsAnonymouslyShared { get; set; }
        public DateTime AnonymouslySharedAtUtc { get; set; }
        public Guid? AnonymousQuestionnaireId { get; set; }
    }
}
