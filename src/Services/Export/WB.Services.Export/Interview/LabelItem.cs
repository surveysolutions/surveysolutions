using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Interview
{
    public class LabelItem
    {
        public LabelItem(Answer answer)
        {
            this.Value = answer.AnswerValue ?? answer.AnswerText;
            this.Title = answer.AnswerText;
        }

        public LabelItem(string value, string title)
        {
            this.Value = value;
            this.Title = title;
        }

        public string Value { get; set; }
        public string Title { get; set; }
    }
}
