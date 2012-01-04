using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Utils
{
    public class CompleteQuestionnaireFactory
    {
        public static CompleteQuestionnaire CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "completequestionnairedocuments/cqID";
            innerDocument.Questionnaire = new QuestionnaireDocument() { Id = "questionnairedocuments/qID" };
            innerDocument.Questionnaire.Questions.Add(new Question()
            {
                QuestionText = "test question",
                QuestionType = QuestionType.SingleOption
            });
            Answer answer = new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select};
            innerDocument.Questionnaire.Questions[0].Add(answer, null);
            Answer answer2 = new Answer() { AnswerText = "answer2", AnswerType = AnswerType.Select };
            innerDocument.Questionnaire.Questions[0].Add(answer2, null);
            return new CompleteQuestionnaire(innerDocument);
        }
    }
}
