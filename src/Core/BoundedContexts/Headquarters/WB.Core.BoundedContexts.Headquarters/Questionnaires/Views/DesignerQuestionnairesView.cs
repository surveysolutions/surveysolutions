using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Views
{
    public class DesignerQuestionnairesView 
    {
        public int TotalCount { get; set; }
        public IEnumerable<DesignerQuestionnaireListViewItem> Items { get; set; }
        public DesignerQuestionnaireListViewItem ItemsSummary { get; set; }
    }
}