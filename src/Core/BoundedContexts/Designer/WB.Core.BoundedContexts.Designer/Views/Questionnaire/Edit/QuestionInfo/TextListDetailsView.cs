using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class TextListDetailsView : QuestionDetailsView
    {
        public TextListDetailsView()
        {
            Type = QuestionType.TextList;
        }
        public int? MaxAnswerCount { get; set; }
        public override sealed QuestionType Type { get; set; }
    }
}