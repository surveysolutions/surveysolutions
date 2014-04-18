using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class TextListDetailsView : QuestionDetailsView
    {
        public readonly QuestionType QuestionType = QuestionType.TextList;
        public int? MaxAnswerCount { get; set; }
    }
}