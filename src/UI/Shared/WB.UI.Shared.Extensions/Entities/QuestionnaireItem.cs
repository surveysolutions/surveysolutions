namespace WB.UI.Shared.Extensions.Entities
{
    public class QuestionnaireItem
    {
        public QuestionnaireItem(string questionnaireId, string title)
        {
            QuestionnaireId = questionnaireId;
            Title = title;
        }

        public string Title {private set; get; }
        public string QuestionnaireId { private set; get; }
    }
}
