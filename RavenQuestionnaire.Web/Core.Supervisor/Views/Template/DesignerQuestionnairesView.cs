using System.Collections.Generic;

namespace Core.Supervisor.Views.Template
{
    public class DesignerQuestionnairesView : IListView<DesignerQuestionnaireListViewItem>
    {
        public int TotalCount { get; set; }
        public IEnumerable<DesignerQuestionnaireListViewItem> Items { get; set; }
        public DesignerQuestionnaireListViewItem ItemsSummary { get; set; }
    }
}