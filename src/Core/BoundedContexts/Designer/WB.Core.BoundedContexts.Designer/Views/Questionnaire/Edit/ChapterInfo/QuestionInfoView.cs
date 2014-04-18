using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class QuestionInfoView : IQuestionnaireItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Variable { get; set; }
        public QuestionType Type { get; set; }
        public IEnumerable<string> LinkedVariables { get; set; }
        public IEnumerable<string> BrokenLinkedVariables { get; set; }
    }
}