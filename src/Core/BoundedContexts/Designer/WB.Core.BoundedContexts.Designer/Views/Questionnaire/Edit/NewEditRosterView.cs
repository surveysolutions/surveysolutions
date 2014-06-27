using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public RosterDetailsView Roster { get; set; }
        public Dictionary<string, QuestionBrief[]> NumericIntegerQuestions { get; set; }
        public Dictionary<string, QuestionBrief[]> NotLinkedMultiOptionQuestions { get; set; }
        public Dictionary<string, QuestionBrief[]> TextListsQuestions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}