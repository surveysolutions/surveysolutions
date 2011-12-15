using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireViewInputModel
    {
        public QuestionnaireViewInputModel(string id)
        {
            QuestionnaireId = IdUtil.CreateQuestionnaireId(id);
        }

        public string QuestionnaireId { get; private set; }
    }
}
