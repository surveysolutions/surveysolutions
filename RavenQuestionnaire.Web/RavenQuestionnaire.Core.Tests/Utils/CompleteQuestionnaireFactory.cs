using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                QuestionnaireId = innerDocument.Questionnaire.Id,
                QuestionText = "test question",
                QuestionType = QuestionType.SingleOption
            });
            Answer answer = new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select};
            innerDocument.Questionnaire.Questions[0].AddAnswer(answer);
            return new CompleteQuestionnaire(innerDocument);
        }
    }
}
