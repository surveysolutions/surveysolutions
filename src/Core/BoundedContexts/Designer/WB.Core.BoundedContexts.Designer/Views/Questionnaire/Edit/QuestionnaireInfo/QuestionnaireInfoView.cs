using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoView
    {
        public QuestionnaireInfoView()
        {
            this.SharedPersons = new List<SharedPersonView>();
            this.Chapters = new List<ChapterInfoView>();
            this.Macros = new List<MacroView>();
            this.LookupTables = new List<LookupTableView>();
            this.Attachments = new List<AttachmentView>();
            this.Translations = new List<TranslationView>();
        }
        public string QuestionnaireId { get; set; }
        public string Title { get; set; }
        public string Variable { get; set; }
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
        public bool HasViewerAdminRights { get; set; }

        public List<AttachmentView> Attachments { get; set; }

        public List<TranslationView> Translations { get; set; }

        public MetadataView Metadata { get; set; }

        public List<StudyTypeItem> StudyTypes { get; set; }
        public List<KindOfDataItem> KindsOfData { get; set; }
        public List<CountryItem> Countries { get; set; }
        public List<ModeOfDataCollectionItem> ModesOfDataCollection { get; set; }
        public bool? HideIfDisabled { get; set; }
    }
}
