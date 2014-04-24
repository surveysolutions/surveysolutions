using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public GroupDetailsView Roster { get; set; }
        public Dictionary<string, QuestionBrief[]> NumericIntegerQuestions { get; set; }
        public Dictionary<string, QuestionBrief[]> NotLinkedMultiOptionQuestions { get; set; }
        public Dictionary<string, QuestionBrief[]> TextListsQuestions { get; set; }
    }
}