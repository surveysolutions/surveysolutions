using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.Headquarters.Models
{
    public class ImportModeModel
    {
        public ImportModeModel()
        {
            this.QuestionnairesToUpgradeFrom = new List<TemplateViewItem>();
        }

        public QuestionnaireInfo QuestionnaireInfo
        {
            get;
            set;
        }

        public long NewVersionNumber { get; set; }
        public DateTime PreviousVersionUploadedDate { get; set; }
        public string ErrorMessage { get; set; }
        public List<TemplateViewItem> QuestionnairesToUpgradeFrom { get; set; }
    }
}
