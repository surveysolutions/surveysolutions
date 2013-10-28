using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void AREventRaisingOnAddGroup()
        {
            Guid key = Guid.NewGuid();

            string title = "test q";
            Guid responsibleId = Guid.NewGuid();
            var questionnaire = new Questionnaire(publicKey: key, title: title, createdBy: responsibleId);

            using (var ctx = new EventContext())
            {
                Guid publicKey = Guid.NewGuid();
                string text = "group 1";
                var propagateble = Propagate.None;
                Guid? parentGroupKey = null;
                string conditionExpression = "1=1";
                string description = "group desct";

                questionnaire.NewAddGroup(publicKey, parentGroupKey, text, propagateble, description, conditionExpression, responsibleId: responsibleId);

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as NewGroupAdded;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.PublicKey, publicKey);
                        Assert.AreEqual(evnt.Paropagateble, propagateble);
                        Assert.AreEqual(evnt.ParentGroupPublicKey, parentGroupKey);
                        Assert.AreEqual(evnt.GroupText, text);
                        Assert.AreEqual(evnt.Description, description);
                        Assert.AreEqual(evnt.ConditionExpression, conditionExpression);

                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        [Test]
        public void AREventRaisingOnAddQuestion()
        {
            Guid key = Guid.NewGuid();

            string title = "test q 22";
            Guid responsibleId = Guid.NewGuid();
            var questionnaire = new Questionnaire(publicKey: key, title: title, createdBy: responsibleId);

            Guid groupPublicKey = Guid.NewGuid();

            questionnaire.NewAddGroup(groupPublicKey, null, "title", Propagate.None, "description", null, responsibleId: responsibleId);

            using (var ctx = new EventContext())
            {
                Guid publicKey = Guid.NewGuid();
                string questionText = "q 1";
                string conditionExpression = "1=1";
                string stataExportCaption = "s1q1";

                var questionType = QuestionType.Text;
                var questionScope = QuestionScope.Interviewer;
                string validationExpression = "2=2";
                string validationMessage = "not valid";
                bool featured = false;
                bool mandatory = true;
                var answerOrder = Order.AsIs;
                string instructions = "do it";
                Option[] answers = null;
                bool capital = false;

                questionnaire.NewAddQuestion(publicKey, groupPublicKey, questionText, questionType,
                    stataExportCaption, mandatory, featured, capital, questionScope, conditionExpression,
                    validationExpression, validationMessage, instructions, answers, answerOrder, 
                    responsibleId: responsibleId, linkedToQuestionId: null, isAnswersOrdered: false, maxAllowedAnswers: null);

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as NewQuestionAdded;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.AnswerOrder, answerOrder);

                        Assert.AreEqual(evnt.Answers, answers);
                        Assert.AreEqual(evnt.PublicKey, publicKey);
                        Assert.AreEqual(evnt.ConditionExpression, conditionExpression);
                        Assert.AreEqual(evnt.Featured, featured);
                        Assert.AreEqual(evnt.GroupPublicKey, groupPublicKey);
                        Assert.AreEqual(evnt.Instructions, instructions);
                        Assert.AreEqual(evnt.Mandatory, mandatory);
                        Assert.AreEqual(evnt.QuestionScope, questionScope);

                        Assert.AreEqual(evnt.QuestionText, questionText);
                        Assert.AreEqual(evnt.QuestionType, questionType);

                        Assert.AreEqual(evnt.StataExportCaption, stataExportCaption);

                        Assert.AreEqual(evnt.ValidationExpression, validationExpression);
                        Assert.AreEqual(evnt.ValidationMessage, validationMessage);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        [Test]
        public void AREventRaisingOnCreated()
        {
            Guid key = Guid.NewGuid();

            string title = "test q";

            using (var ctx = new EventContext())
            {
                var questionnaire = new Questionnaire(key, title);

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as NewQuestionnaireCreated;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.PublicKey, key);
                        Assert.AreEqual(evnt.Title, title);

                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        [Test]
        public void AREventRaisingOnUpdateQuestionnaire()
        {
            Guid key = Guid.NewGuid();

            string title = "test q";
            string title1 = "test q";
            Guid responsibleId = Guid.NewGuid();
            var questionnaire = new Questionnaire(publicKey: key, title: title, createdBy: responsibleId);

            using (var ctx = new EventContext())
            {
                questionnaire.UpdateQuestionnaire(title: title1, isPublic: false, responsibleId: responsibleId);

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as QuestionnaireUpdated;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.Title, title1);

                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        [SetUp]
        public void CreateObjects()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }
    }
}