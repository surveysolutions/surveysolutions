namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireId
    {
        public QuestionnaireId(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

        public override string ToString()
        {
            return Id;
        }
    }
}
