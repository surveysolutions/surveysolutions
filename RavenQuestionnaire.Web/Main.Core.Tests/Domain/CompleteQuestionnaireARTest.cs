namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire.Completed;

    using Ncqrs.Eventing;
    using Ncqrs.Spec;

    using NUnit.Framework;

    /// <summary>
    /// The complete questionnaire  test.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireARTest
    {
        #region Fields

        /// <summary>
        /// The document.
        /// </summary>
        private TestDataConfigurator configurator;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The ar event raising on answer set.
        /// </summary>
        [Test]
        public void AREventRaisingOnAnswerSet()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.configurator.Document, null);

            using (var ctx = new EventContext())
            {
                completeQuestionnaire.SetAnswer(
                    this.configurator.QuestionKey, 
                    null, 
                    null, 
                    new List<Guid> { this.configurator.Answer1Key }, 
                    DateTime.UtcNow);

                Assert.True(ctx.Events.Any());

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var answerSetEvent = item.Payload as AnswerSet;
                    if (answerSetEvent != null)
                    {
                        Assert.AreEqual(answerSetEvent.QuestionPublicKey, this.configurator.QuestionKey);
                        Assert.AreEqual(answerSetEvent.AnswerKeys.Count(), 1);
                        Assert.AreEqual(answerSetEvent.AnswerKeys[0], this.configurator.Answer1Key);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on answer set with propagation.
        /// </summary>
        [Test]
        public void AREventRaisingOnAnswerSetWithPropagation()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.configurator.Document, null);

            using (var ctx = new EventContext())
            {
                completeQuestionnaire.SetAnswer(this.configurator.AutoPropQuestionKey, null, "1", null, DateTime.UtcNow);

                Assert.True(ctx.Events.Any());

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var answerSetEvent = item.Payload as AnswerSet;
                    if (answerSetEvent != null)
                    {
                        Assert.AreEqual(answerSetEvent.QuestionPublicKey, this.configurator.AutoPropQuestionKey);
                        Assert.AreEqual(answerSetEvent.AnswerKeys, null);
                        Assert.AreEqual(answerSetEvent.AnswerValue, "1");

                        // Assert.AreEqual(answerSetEvent.AnswerKeys[0], this.answer1Key);
                        continue;
                    }

                    var propagatableEvent = item.Payload as PropagatableGroupAdded;
                    if (propagatableEvent != null)
                    {
                        Assert.AreEqual(propagatableEvent.ParentKey, this.configurator.MainGroupKey);
                        Assert.AreEqual(propagatableEvent.ParentPropagationKey, null);
                        Assert.AreEqual(propagatableEvent.PublicKey, this.configurator.AutoGroupKey);

                        continue;
                    }

                    var statusEvent = item.Payload as ConditionalStatusChanged;
                    if (statusEvent != null)
                    {
                        // Assert.AreEqual(statusEvent.CompletedQuestionnaireId, this.mainGroupKey);
                        Assert.AreEqual(statusEvent.ResultGroupsStatus.Count, 1);
                        Assert.AreEqual(statusEvent.ResultQuestionsStatus.Count, 1);

                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on change assignment.
        /// </summary>
        [Test]
        public void AREventRaisingOnChangeAssignment()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.configurator.Document, null);

            using (var ctx = new EventContext())
            {
                Guid userId = Guid.NewGuid();
                string userName = "test";

                completeQuestionnaire.ChangeAssignment(new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as QuestionnaireAssignmentChanged;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.CompletedQuestionnaireId, key);
                        Assert.AreEqual(evnt.Responsible.Id, userId);
                        Assert.AreEqual(evnt.Responsible.Name, userName);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on status change.
        /// </summary>
        [Test]
        public void AREventRaisingOnCommentSet()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.configurator.Document, null);

            using (var ctx = new EventContext())
            {
                string commentText = "test comment";

                completeQuestionnaire.SetComment(this.configurator.QuestionKey, commentText, null, new UserLight(Guid.NewGuid(), "User"));

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as CommentSet;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.QuestionPublickey, this.configurator.QuestionKey);
                        Assert.AreEqual(evnt.Comments, commentText);
                        Assert.AreEqual(evnt.PropagationPublicKey, null);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on status change.
        /// </summary>
        [Test]
        public void AREventRaisingOnCreated()
        {
            Guid key = Guid.NewGuid();

            Guid userId = Guid.NewGuid();
            string userName = "testUser";

            using (var ctx = new EventContext())
            {
                var completeQuestionnaire = new CompleteQuestionnaireAR(
                    key, this.configurator.Document, new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as NewCompleteQuestionnaireCreated;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.Questionnaire.PublicKey, key);
                        Assert.AreEqual(evnt.Questionnaire.Status, SurveyStatus.Unknown);
                        Assert.AreEqual(evnt.Questionnaire.Creator.Id, userId);
                        Assert.AreEqual(evnt.Questionnaire.Creator.Name, userName);

                        Assert.AreEqual(evnt.Questionnaire.Responsible, null);

                        Assert.AreEqual(evnt.TotalQuestionCount, 3);

                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on status change.
        /// </summary>
        [Test]
        public void AREventRaisingOnDeleted()
        {
            Guid key = Guid.NewGuid();

            Guid userId = Guid.NewGuid();
            string userName = "testUser";

            var completeQuestionnaire = new CompleteQuestionnaireAR(
                key, this.configurator.Document, new UserLight(userId, userName));

            using (var ctx = new EventContext())
            {
                completeQuestionnaire.Delete();

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as CompleteQuestionnaireDeleted;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.CompletedQuestionnaireId, key);
                        Assert.AreEqual(evnt.TemplateId, this.configurator.Document.PublicKey);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The ar event raising on status change.
        /// </summary>
        [Test]
        public void AREventRaisingOnStatusChange()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.configurator.Document, null);

            using (var ctx = new EventContext())
            {
                Guid userId = Guid.NewGuid();
                string userName = "testUser";

                completeQuestionnaire.ChangeStatus(SurveyStatus.Initial, new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (UncommittedEvent item in ctx.Events)
                {
                    var evnt = item.Payload as QuestionnaireStatusChanged;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.CompletedQuestionnaireId, key);
                        Assert.AreEqual(evnt.Responsible.Id, userId);
                        Assert.AreEqual(evnt.Responsible.Name, userName);
                        Assert.AreEqual(evnt.Status, SurveyStatus.Initial);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.configurator = new TestDataConfigurator();
        }

        #endregion
    }
}