using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class CompleteQuestionnaireTest
    {
        [SetUp]
        public void CreateObjects()
        {
            IKernel kernel = new StandardKernel();
            subscriber = new Subscriber(kernel);
            kernel.Bind<IEntitySubscriber<ICompleteGroup>>().To<PropagationAddedSubscriber>();
            kernel.Bind<IEntitySubscriber<ICompleteGroup>>().To<PropagationRemovedSubscriber>();
            kernel.Bind<IEntitySubscriber<ICompleteGroup>>().To<ScopePropagationSubscriber>();
        
            kernel.Bind<IEntitySubscriber<ICompleteGroup>>().To<BindedQuestionSubscriber>();
        }

        private ISubscriber subscriber;
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
                                    Title = innerDocument.Questions[0].Answers[0].Title
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
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);

            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Children.Count, 2);
            Assert.AreEqual(qDoqument.Children[0].PublicKey, qDoqument.Children[1].PublicKey);
            Assert.True(((ICompleteGroup)qDoqument.Children[1]).PropogationPublicKey.HasValue);
        }
        [Test]
        public void PropogateGroup_InValidDataNotPropogatebleGroup_GroupIsNotAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test");
            SingleCompleteQuestion question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            Assert.Throws<CompositeException>(() => questionanire.Add(group, null));
        }
        [Test]
        public void PropogateGroup_ValidDataOtherGroupIsSubscribed_GroupIsAddedOtherGroupIsnotified()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();

            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            CompleteGroup otherGroup = new CompleteGroup("other") { Propagated = Propagate.Propagated };
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            qDoqument.Children.Add(otherGroup);
            otherGroup.Triggers.Add(group.PublicKey);
          //  qDoqument.Observers = new List<IObserver<CompositeInfo>> { new GroupObserver(otherGroup.PublicKey, group.PublicKey) };

            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument, subscriber);
        
            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Children.Count, 4);
            Assert.AreEqual(qDoqument.Children[0].PublicKey, qDoqument.Children[2].PublicKey);
            Assert.AreEqual(qDoqument.Children[1].PublicKey, qDoqument.Children[3].PublicKey);
            Assert.True(((ICompleteGroup)qDoqument.Children[2]).PropogationPublicKey.HasValue);
            Assert.True(((ICompleteGroup)qDoqument.Children[3]).PropogationPublicKey.HasValue);
        }
        [Test]
        public void Add_AnswerInPropogatedGroup_AnswerIsAdded()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            questionanire.Add(group, null);
            questionanire.Add(group, null);
            CompleteAnswer completeAnswer = new CompleteAnswer(answer);

            questionanire.Add(
                new CompleteAnswer(completeAnswer,
                                               ((CompleteGroup)qDoqument.Children[1]).PropogationPublicKey.Value),
                null);
            Assert.AreEqual((((qDoqument.Children[0] as CompleteGroup).Children[0] as AbstractCompleteQuestion).Children[0] as CompleteAnswer).Selected, false);
            Assert.AreEqual((((qDoqument.Children[1] as CompleteGroup).Children[0] as AbstractCompleteQuestion).Children[0] as CompleteAnswer).Selected, true);
            Assert.AreEqual((((qDoqument.Children[2] as CompleteGroup).Children[0] as AbstractCompleteQuestion).Children[0] as CompleteAnswer).Selected, false);

        }

        [Test]
        public void RemovePropogatedGroup_GroupIsValid_GroupIsRemoved()
        {

            CompleteQuestionnaireDocument qDoqument = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            CompleteGroup group = new CompleteGroup("test") { Propagated = Propagate.Propagated };
            var question = new SingleCompleteQuestion("q");
            CompleteAnswer answer = new CompleteAnswer(new Answer());
            question.Children.Add(answer);
            group.Children.Add(question);
            qDoqument.Children.Add(group);
            questionanire.Add(group, null);

            Assert.AreEqual(qDoqument.Children.Count, 2);
            Assert.IsTrue(((CompleteGroup) qDoqument.Children[1]).
                              PropogationPublicKey.HasValue);
            questionanire.Remove(new CompleteGroup(group,
                                                               ((CompleteGroup)qDoqument.Children[1]).
                                                                   PropogationPublicKey.Value));
            Assert.AreEqual(qDoqument.Children.Count, 1);
            Assert.AreEqual(qDoqument.Children[0].GetType(), typeof(CompleteGroup));

        }
        [Test]
        public void FindAllIComposite_Success_AllGroupsQuestionsAndAnswersIsReturned()
        {
            QuestionnaireDocument questionnaireInnerDocument = new QuestionnaireDocument();
            //queston without group
            questionnaireInnerDocument.Id = "completequestionnairedocuments/cqID";
            var testQuestion1 = new SingleQuestion("test question");
            questionnaireInnerDocument.Children.Add(testQuestion1);
            Answer answer = new Answer() {AnswerText = "answer", AnswerType = AnswerType.Select};
            testQuestion1.Add(answer, null);
            Answer answer2 = new Answer() {AnswerText = "answer2", AnswerType = AnswerType.Select};
            testQuestion1.Add(answer2, null);
            //group
            Group group = new Group("group");
            var testQuestion2 = new SingleQuestion("test question");
            group.Children.Add(testQuestion2);
            testQuestion2.Add(new Answer() { AnswerText = "answer", AnswerType = AnswerType.Select }, null);
            testQuestion2.Add(new Answer() { AnswerText = "answer2", AnswerType = AnswerType.Select }, null);
            questionnaireInnerDocument.Children.Add(group);

            //group for propagation
            Group groupPropogated = new Group("group") {Propagated = Propagate.Propagated};
            var testQuestion3 = new SingleQuestion("test question");
            groupPropogated.Children.Add(testQuestion3);
            testQuestion3.Add(new Answer() { AnswerText = "answer", AnswerType = AnswerType.Select }, null);
            testQuestion3.Add(new Answer() { AnswerText = "answer2", AnswerType = AnswerType.Select }, null);
            group.Add(groupPropogated, null);

            CompleteQuestionnaire completeQuestionnaire =
                new CompleteQuestionnaire(new Questionnaire(questionnaireInnerDocument), Guid.NewGuid(), new UserLight(),
                                          new SurveyStatus(), subscriber);
            CompleteQuestionnaireDocument innerDocument =
                ((IEntity<CompleteQuestionnaireDocument>) completeQuestionnaire).GetInnerDocument();
            var result = completeQuestionnaire.Find<IComposite>(c => true);
            Assert.AreEqual(result.Count(), 11);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteAnswer>(c => true).Count(), 6);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteQuestion>(c => true).Count(), 3);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteGroup>(c => true).Count(), 2);

            completeQuestionnaire.Add((CompleteGroup)groupPropogated, null);
            Assert.AreEqual(completeQuestionnaire.Find<IComposite>(c => true).Count(), 15);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteGroup>(c => c.PropogationPublicKey.HasValue).Count(), 1);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteQuestion>(c => c.PropogationPublicKey.HasValue).Count(), 1);
            Assert.AreEqual(completeQuestionnaire.Find<ICompleteAnswer>(c => c.PropogationPublicKey.HasValue).Count(), 2);
        }
        [Test]
        public void ExplicitConversion_ValidQuestionneir_AllFieldAreConverted()
        {
            List<IComposite> children = new List<IComposite>() { new Group("test"), new SingleQuestion("question") };
          

            List<Guid> triggers = new List<Guid>() { Guid.NewGuid() };
            QuestionnaireDocument doc = new QuestionnaireDocument()
                                            {
                                                Id = "test",
                                                Propagated = Propagate.Propagated,
                                                Title = "new title",
                                                Children = children,
                                                Triggers = triggers
                                            };
            CompleteQuestionnaireDocument target = (CompleteQuestionnaireDocument) doc;
            var propertiesForCheck =
                typeof (IQuestionnaireDocument).GetPublicPropertiesExcept("Id", "CreationDate", "LastEntryDate",
                                                                          "OpenDate", "CloseDate", "Parent", "Children","PublicKey");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {

                Assert.AreEqual(publicProperty.GetValue(doc, null), publicProperty.GetValue(target, null));
            }
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
