using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Template
{
    public class DesignerQuestionnairesView : IListView<DesignerQuestionnaireListViewItem>
    {
        public int TotalCount { get; set; }
        public IEnumerable<DesignerQuestionnaireListViewItem> Items { get; set; }
    }
}