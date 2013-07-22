using Main.Core.Documents;

namespace Main.Core.Events.Questionnaire.Completed
{
    public class NewAssigmentCreated
    {
        public CompleteQuestionnaireDocument Source { get; set; }
    }
}
