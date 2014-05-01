using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditQuestionView
    {
        public QuestionDetailsView Question { get; set; }
        public Dictionary<string, QuestionBrief[]> SourceOfLinkedQuestions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] QuestionTypeOptopns { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] QuestionScopeOptions { get; set; }
        public GroupBrief[] Breadcrumbs { get; set; }
    }
}