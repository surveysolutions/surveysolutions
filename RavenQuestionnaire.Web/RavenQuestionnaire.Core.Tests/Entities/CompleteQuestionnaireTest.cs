using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CompleteQuestionnaireTest
    {
        [SetUp]
        public void CreateObjects()
        {
        }
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
        public void PropogateGroup_ValidData_GroupIsAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
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
            Assert.Throws<CompositeException>(() => questionanire.Add(group, null));
        }
        [Test]
        public void PropogateGroup_ValidDataOtherGroupIsSubscribed_GroupIsAddedOtherGroupIsnotified()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();

            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            CompleteGroup otherGroup = new CompleteGroup("other") { Propagated = Propagate.Propagated };
            CompleteQuestion question = new CompleteQuestion("q",
                                           QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            group.Questions.Add(question);
            qDoqument.Groups.Add(group);
            qDoqument.Groups.Add(otherGroup);
            qDoqument.Observers = new List<IObserver<CompositeInfo>> { new GroupObserver(otherGroup.PublicKey, group.PublicKey) };

            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
        
            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Groups.Count, 4);
            Assert.AreEqual(qDoqument.Groups[0].PublicKey, qDoqument.Groups[2].PublicKey);
            Assert.AreEqual(qDoqument.Groups[1].PublicKey, qDoqument.Groups[3].PublicKey);
            Assert.True(qDoqument.Groups[2] is IPropogate);
            Assert.True(qDoqument.Groups[3] is IPropogate);
        }
        [Test]
        public void Add_AnswerInPropogatedGroup_AnswerIsAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
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
            Assert.AreEqual((((qDoqument.Groups[0] as CompleteGroup).Questions[0] as CompleteQuestion).Answers[0] as CompleteAnswer).Selected, false);
            Assert.AreEqual((((qDoqument.Groups[1] as CompleteGroup).Questions[0] as CompleteQuestion).Answers[0] as CompleteAnswer).Selected, true);
            Assert.AreEqual((((qDoqument.Groups[2] as CompleteGroup).Questions[0] as CompleteQuestion).Answers[0] as CompleteAnswer).Selected, false);

        }

        [Test]
        public void RemovePropogatedGroup_GroupIsValid_GroupIsRemoved()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
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
        [Test]
        public void FindAllIComposite_Success_AllGroupsQuestionsAndAnswersIsReturned()
        {
            QuestionnaireDocument questionnaireInnerDocument = new QuestionnaireDocument();
            //queston without group
            questionnaireInnerDocument.Id = "completequestionnairedocuments/cqID";
            questionnaireInnerDocument.Questions.Add(new Question("test question", QuestionType.SingleOption));
            Answer answer = new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select};
            questionnaireInnerDocument.Questions[0].Add(answer, null);
            Answer answer2 = new Answer() {AnswerText = "answer2", AnswerType = AnswerType.Select};
            questionnaireInnerDocument.Questions[0].Add(answer2, null);
            //group
            Group group = new Group("group");
            group.Questions.Add(new Question("test question", QuestionType.SingleOption));
            group.Questions[0].Add(new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select}, null);
            group.Questions[0].Add(new Answer() {AnswerText = "answer2", AnswerType = AnswerType.Select}, null);
            questionnaireInnerDocument.Groups.Add(group);

            //group for propagation
            Group groupPropogated = new Group("group") {Propagated = Propagate.Propagated};
            groupPropogated.Questions.Add(new Question("test question", QuestionType.SingleOption));
            groupPropogated.Questions[0].Add(new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select}, null);
            groupPropogated.Questions[0].Add(new Answer() {AnswerText = "answer2", AnswerType = AnswerType.Select}, null);
            questionnaireInnerDocument.Groups[0].Add(groupPropogated, null);

            CompleteQuestionnaire completeQuestionnaire =
                new CompleteQuestionnaire(new Questionnaire(questionnaireInnerDocument), new UserLight(),
                                          new SurveyStatus());
            CompleteQuestionnaireDocument innerDocument =
                ((IEntity<CompleteQuestionnaireDocument>) completeQuestionnaire).GetInnerDocument();
            var result = completeQuestionnaire.Find<IComposite>(c => true);
            Assert.AreEqual(result.Count(), 11);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteAnswer>(c => true).Count(), 6);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteQuestion>(c => true).Count(), 3);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteGroup>(c => true).Count(), 2);

            completeQuestionnaire.Add(((CompleteGroup)innerDocument.Groups[0]).Groups[0], null);
            Assert.AreEqual(completeQuestionnaire.Find<IComposite>(c => true).Count(), 15);
            Assert.AreEqual(completeQuestionnaire.Find<IPropogate>(c => true).Count(), 4);
        }

        /*   [Test]
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
