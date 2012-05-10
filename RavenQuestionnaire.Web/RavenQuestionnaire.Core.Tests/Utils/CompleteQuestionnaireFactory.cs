using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Tests.Utils
{
    public class CompleteQuestionnaireFactory
    {
        public static CompleteQuestionnaire CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "completequestionnairedocuments/cqID";
            innerDocument.Children.Add(new Question("test question", QuestionType.SingleOption));
            Answer answer = new Answer() { AnswerText = "answer", AnswerType = AnswerType.Select };
            innerDocument.Children[0].Add(answer, null);
            Answer answer2 = new Answer() { AnswerText = "answer2", AnswerType = AnswerType.Select };
            innerDocument.Children[0].Add(answer2, null);
            return new CompleteQuestionnaire(new Questionnaire(innerDocument), new UserLight(), new SurveyStatus());
        }
    }
}
