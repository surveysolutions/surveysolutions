using System;
using System.Collections.Generic;
using System.Data;
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

            Assert.Throws<InvalidOperationException>(() => questionnaire.AddAnswer(new CompleteAnswer() { PublicKey = Guid.NewGuid(),CustomAnswer = "test" }));
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
            completeQuestionnaire.AddAnswer(new CompleteAnswer(innerDocument.Questionnaire.Questions[0].Answers[0], innerDocument.Questionnaire.Questions[0].PublicKey));
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
        [Test]
        public void GetAllAnswers_ReturnsAllCompleetAnswerList()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var answers = new List<CompleteAnswer>();
            innerDocument.CompletedAnswers = answers;
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers(), answers);
        }
        [Test]
        public void GetAllQuestions_ReturnsAllQuestions()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var questions = new List<Question>();
            innerDocument.Questionnaire = new QuestionnaireDocument();
            innerDocument.Questionnaire.Questions = questions;
            Assert.AreEqual(completeQuestionnaire.GetAllQuestions(), questions);
        }

        [Test]
        public void UpdateAnswer_UpdateUnpresentedQuestion_ExceptionIsThrownen()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            Assert.Throws<InvalidOperationException>(
                () => completeQuestionnaire.UpdateAnswer(new CompleteAnswer(new Answer(), Guid.NewGuid())));

        }
        [Test]
        public void UpdateAnswer_UpdateQuestion_QuestionIsUpdated()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questionnaire.Questions[0];
            innerDocument.CompletedAnswers.Add(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey));
            

            completeQuestionnaire.UpdateAnswer(new CompleteAnswer(firstQuestion.Answers[1], firstQuestion.PublicKey));
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 1);
            Assert.AreEqual(innerDocument.CompletedAnswers[0].PublicKey, firstQuestion.Answers[1].PublicKey);

        }

        [Test]
        public void AddAnswer_WithEmptyGuid_DoNothing()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            completeQuestionnaire.AddAnswer(new CompleteAnswer() {PublicKey = Guid.Empty});
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 0);

        }
        [Test]
        public void AddAnswer_Dublicate_DuplicateNameExceptionIsThrowed()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questionnaire.Questions[0];
            innerDocument.CompletedAnswers.Add(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey));
            
            Assert.Throws<DuplicateNameException>(
                () =>
                completeQuestionnaire.AddAnswer(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey)));

        }
        [Test]
        public void AddAnswer_UpresentedAnswer_InvalidOperationExceptionIsThrowed()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questionnaire.Questions[0];


            Assert.Throws<InvalidOperationException>(
                () =>
                completeQuestionnaire.AddAnswer(new CompleteAnswer(new Answer(), firstQuestion.PublicKey)));

        }
        [Test]
        public void AddAnswer_CorrectAnswer_AnswerIsAdded()
        {
            CompleteQuestionnaire completeQuestionnaire =
                CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
                ((IEntity<CompleteQuestionnaireDocument>) completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questionnaire.Questions[0];

            completeQuestionnaire.AddAnswer(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey));
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 1);
            Assert.AreEqual(innerDocument.CompletedAnswers[0].PublicKey, firstQuestion.Answers[0].PublicKey);

        }
    }
}
