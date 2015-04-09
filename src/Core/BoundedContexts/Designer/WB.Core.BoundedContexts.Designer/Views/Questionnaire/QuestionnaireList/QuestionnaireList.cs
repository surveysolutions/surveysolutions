using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireList
    {
        public int TotalCount { get; set; }
        public IEnumerable<QuestionnaireListItem> Items { get; set; }
    }
}
