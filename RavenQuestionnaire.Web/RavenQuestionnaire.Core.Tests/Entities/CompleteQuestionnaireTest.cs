using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CompleteQuestionnaireTest
    {
  /*      [Test]
        public void WhenAddCompletedAnswerNotInQuestionnaireList_InvalidExceptionThrowed()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionnaire = new CompleteQuestionnaire(innerDocument);

            Assert.Throws<InvalidOperationException>(
                () =>
                questionnaire.AddAnswer(Guid.NewGuid(),  "test"));
        }
        [Test]
        public void WhenAddCompletedAnswerFromQuestionnaireList_AnswerIsAdded()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ( (IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            Answer answer = new Answer()
                                {
                                    PublicKey = innerDocument.Questions[0].Answers[0].PublicKey,
                                    AnswerText = innerDocument.Questions[0].Answers[0].AnswerText
                                };
            completeQuestionnaire.AddAnswer(innerDocument.Questions[0].Answers[0].PublicKey,
                                            "custom text");
            Assert.AreEqual(innerDocument.Questions[0].Answers[0].CustomAnswer, "custom text");
            Assert.AreEqual(innerDocument.Questions[0].Answers[0].Selected, true);
        }

        [Test]
        public void ClearAnswers_ClearsCompletedAnswersToDocument()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            completeQuestionnaire.ClearAnswers();
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers().Count(), 0);
        }
        [Test]
        public void GetAllAnswers_ReturnsAllCompleetAnswerList()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var answers = new List<CompleteAnswer>();
            completeQuestionnaire.AddAnswer(innerDocument.Questions[0].Answers[0].PublicKey,
                                           "custom text");
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers().Count(), 1);
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers().First().PublicKey, innerDocument.Questions[0].Answers[0].PublicKey);
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers().First().CustomAnswer, "custom text");
        }*/
        [Test]
        public void GetAllQuestions_ReturnsAllQuestions()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var questions = new List<CompleteQuestion>();
            innerDocument.Questions = questions;
            Assert.AreEqual(completeQuestionnaire.QuestionIterator, questions);
        }
        [Test]
        public void PropogateGroup_ValidData_GroupIsAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = true };
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);

            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Groups.Count, 2);
            Assert.AreEqual(qDoqument.Groups[0].PublicKey, qDoqument.Groups[1].PublicKey);
            Assert.True(qDoqument.Groups[1] is IPropogate);
        }
        [Test]
        public void PropogateGroup_InValidDataNotPropogatebleGroup_GroupIsNotAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test");
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            questionanire.Add(group, null);
            Assert.AreEqual(qDoqument.Groups.Count, 1);
        }

        [Test]
        public void Add_AnswerInPropogatedGroup_AnswerIsAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = true };
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            questionanire.Add(group, null);
            questionanire.Add(group, null);
            CompleteAnswer completeAnswer = new CompleteAnswer(answer, question.PublicKey);

            questionanire.Add(
                new PropagatableCompleteAnswer(completeAnswer,
                                               ((PropagatableCompleteGroup) qDoqument.Groups[1]).PropogationPublicKey),
                null);
            Assert.AreEqual(qDoqument.Groups[0].Questions[0].Answers[0].Selected, false);
            Assert.AreEqual(qDoqument.Groups[1].Questions[0].Answers[0].Selected, true);
            Assert.AreEqual(qDoqument.Groups[2].Questions[0].Answers[0].Selected, false);

        }

        [Test]
        public void RemovePropogatedGroup_GroupIsValid_GroupIsRemoved()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = true };
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Groups.Count, 2);
            Assert.AreEqual(qDoqument.Groups[1].GetType(), typeof (PropagatableCompleteGroup));
            questionanire.Remove(new PropagatableCompleteGroup(group,
                                                               ((PropagatableCompleteGroup) qDoqument.Groups[1]).
                                                                   PropogationPublicKey));
            Assert.AreEqual(qDoqument.Groups.Count, 1);
            Assert.AreEqual(qDoqument.Groups[0].GetType(), typeof(CompleteGroup));

        }
     /*   [Test]
        public void UpdateAnswer_UpdateUnpresentedQuestion_ExceptionIsThrownen()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            Assert.Throws<InvalidOperationException>(
                () => completeQuestionnaire.ChangeAnswer(new CompleteAnswer(new Answer(), Guid.NewGuid())));

        }
        [Test]
        public void UpdateAnswer_UpdateQuestion_QuestionIsUpdated()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questions[0];
            firstQuestion.SetAnswer(firstQuestion.Answers[0].PublicKey, firstQuestion.Answers[0].CustomAnswer);


            completeQuestionnaire.ChangeAnswer(firstQuestion.Answers[1]);
            Assert.AreEqual(innerDocument.Questions[0].Answers[1].Selected, true);

        }

        [Test]
        public void AddAnswer_WithEmptyGuid_DoNothing()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();

            completeQuestionnaire.AddAnswer(Guid.Empty, null);
            Assert.AreEqual(completeQuestionnaire.GetAllAnswers().Count(), 0);

        }*/
   /*     [Test]
        public void AddAnswer_Dublicate_DuplicateNameExceptionIsThrowed()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questions[0];
            innerDocument.CompletedAnswers.Add(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey));
            
            Assert.Throws<DuplicateNameException>(
                () =>
                completeQuestionnaire.AddAnswer(new CompleteAnswer(firstQuestion.Answers[0], firstQuestion.PublicKey), null));

        }*/
      /*  [Test]
        public void AddAnswer_UpresentedAnswer_InvalidOperationExceptionIsThrowed()
        {
            CompleteQuestionnaire completeQuestionnaire = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questions[0];


            Assert.Throws<InvalidOperationException>(
                () =>
                completeQuestionnaire.AddAnswer(Guid.NewGuid(), null));

        }
        [Test]
        public void AddAnswer_CorrectAnswer_AnswerIsAdded()
        {
            CompleteQuestionnaire completeQuestionnaire =
                CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
                ((IEntity<CompleteQuestionnaireDocument>) completeQuestionnaire).GetInnerDocument();
            var firstQuestion = innerDocument.Questions[0];

            completeQuestionnaire.AddAnswer(firstQuestion.Answers[0].PublicKey, null);
            Assert.AreEqual(firstQuestion.Answers[0].Selected, true);

        }*/
    }
}
