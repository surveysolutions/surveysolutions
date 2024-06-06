using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Models
{
    public class ImportModeModel
    {
        public ImportModeModel()
        {
            this.QuestionnairesToUpgradeFrom = new List<ComboboxOptionModel>();
        }

        public QuestionnaireInfo QuestionnaireInfo
        {
            get;
            set;
        }

        public long NewVersionNumber { get; set; }
        public DateTime PreviousVersionUploadedDate { get; set; }
        public string ErrorMessage { get; set; }
        public List<ComboboxOptionModel> QuestionnairesToUpgradeFrom { get; set; }
        public string SurveySetupUrl { get; set; }
        public string ListOfMyQuestionnaires { get; set; }
        public string CheckImportingStatus { get; set; }
        
        public ComboboxViewItem[] CriticalityLevels { get; set; }
    }
}
