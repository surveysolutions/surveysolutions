using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;


namespace RavenQuestionnaire.Core.Tests.Utils
{
    public class CompleteQuestionnaireFactory
    {
        public static CompleteQuestionnaire CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.PublicKey = Guid.NewGuid();
            innerDocument.Children.Add(new SingleQuestion(Guid.NewGuid(), "test question"));
            Answer answer = new Answer() { AnswerText = "answer", AnswerType = AnswerType.Select };
            innerDocument.Children[0].Add(answer, null);
            Answer answer2 = new Answer() { AnswerText = "answer2", AnswerType = AnswerType.Select };
            innerDocument.Children[0].Add(answer2, null);
            return new CompleteQuestionnaire(new Questionnaire(innerDocument),Guid.NewGuid(), new UserLight(), new SurveyStatus());
        }
    }
}
