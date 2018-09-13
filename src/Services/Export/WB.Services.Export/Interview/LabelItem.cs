using Main.Core.Entities.SubEntities;

namespace WB.Services.Export.Interview
{
    public class LabelItem
    {
        public LabelItem()
        {
        }

        public LabelItem(Answer answer)
        {
            this.Caption = answer.AnswerValue ?? answer.AnswerText;
            this.Title = answer.AnswerText;
        }

        public string Caption { get; set; }
        public string Title { get; set; }
    }
}
