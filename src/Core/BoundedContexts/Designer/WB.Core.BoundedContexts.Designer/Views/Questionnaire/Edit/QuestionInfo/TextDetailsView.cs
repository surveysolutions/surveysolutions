using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class TextDetailsView : QuestionDetailsView
    {
        public TextDetailsView()
        {
            Type = QuestionType.Text;
        }
        public override QuestionType Type { get; set; }
        public string Mask { get; set; }
    }
}