using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CompleteQuestionnaireTest
    {
        [Test]
        public void WhenAddCompletedAnswerNotInQuestionnaireList_InvalidExceptionThrowed()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Questionnaire= new QuestionnaireDocument();
            CompleteQuestionnaire questionnaire = new CompleteQuestionnaire(innerDocument);

            Assert.Throws<InvalidOperationException>(() => questionnaire.AddAnswer(new CompleteAnswer() { CustomAnswer = "test" }));
        }
        [Test]
        public void WhenAddCompletedAnswerFromQuestionnaireListWithCustonText_InvalidExceptionThrowed()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            Answer answer = new Answer()
            {
                PublicKey = innerDocument.Questionnaire.Questions[0].Answers[0].PublicKey,
                AnswerText = innerDocument.Questionnaire.Questions[0].Answers[0].AnswerText
            };
            Assert.Throws<InvalidOperationException>(
                () =>
                completeQuestionnaire.AddAnswer(new CompleteAnswer(innerDocument.Questionnaire.Questions[0].Answers[0])
                                                    {CustomAnswer = "test"}));
            /*      completeQuestionnaire.AddAnswer(new CompleteAnswer(innerDocument.Questionnaire.Questions[0].Answers[0]));
            Assert.AreEqual(innerDocument.CompletedAnswers[0].CustomAnswer, "answer");*/
        }
        [Test]
        public void WhenAddCompletedAnswerFromQuestionnaireList_AnswerIsAdded()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ( (IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            Answer answer = new Answer()
                                {
                                    PublicKey = innerDocument.Questionnaire.Questions[0].Answers[0].PublicKey,
                                    AnswerText = innerDocument.Questionnaire.Questions[0].Answers[0].AnswerText
                                };
            completeQuestionnaire.AddAnswer(new CompleteAnswer(innerDocument.Questionnaire.Questions[0].Answers[0]));
            Assert.AreEqual(innerDocument.CompletedAnswers[0].PublicKey, innerDocument.Questionnaire.Questions[0].Answers[0].PublicKey);
        }

        [Test]
        public void ClearAnswers_ClearsCompletedAnswersToDocument()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            completeQuestionnaire.ClearAnswers();
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 0);
        }
        [Test]
        public void UpdateAnswerList_UpdatesCompletedAnswersInDocument()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            completeQuestionnaire.UpdateAnswerList(new CompleteAnswer[0]);
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 0);
        }
        [Test]
        public void GetQuestionnaireTemplate_ReturntTemplateForCompleteQuestionnaireFromDocument()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            Questionnaire questionnaire = completeQuestionnaire.GetQuestionnaireTemplate();
            Assert.AreEqual(questionnaire.QuestionnaireId, innerDocument.Questionnaire.Id);
        }
    }
}
