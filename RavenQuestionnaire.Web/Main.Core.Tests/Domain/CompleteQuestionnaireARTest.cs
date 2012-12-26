// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireARTest.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire ar test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.Events.Questionnaire.Completed;

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
        /// The answer 1 key.
        /// </summary>
        private readonly Guid answer1Key = Guid.NewGuid();

        /// <summary>
        /// The answer 2 key.
        /// </summary>
        private readonly Guid answer2Key = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid questionKey = Guid.NewGuid();


        /// <summary>
        /// The answer 1 key.
        /// </summary>
        private readonly Guid autoAnswer1Key = Guid.NewGuid();

        /// <summary>
        /// The answer 2 key.
        /// </summary>
        private readonly Guid autoAnswer2Key = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid autoQuestionKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid autoGroupKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid autoPropQuestionKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid mainGroupKey = Guid.NewGuid();
        

        /// <summary>
        /// The document.
        /// </summary>
        private QuestionnaireDocument document;

        #endregion

        #region Public Methods and Operators


        [Test]
        public void AREventRaisingOnAnswerSet()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();
            
            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);

            using (var ctx = new EventContext())
            {
                
                completeQuestionnaire.SetAnswer(
                this.questionKey, null, null, new List<Guid> { this.answer1Key }, DateTime.UtcNow);

                Assert.True(ctx.Events.Any());

                foreach (var item in ctx.Events)
                {
                    var answerSetEvent = item.Payload as AnswerSet;
                    if (answerSetEvent != null)
                    {
                        Assert.AreEqual(answerSetEvent.QuestionPublicKey, this.questionKey);
                        Assert.AreEqual(answerSetEvent.AnswerKeys.Count(), 1);
                        Assert.AreEqual(answerSetEvent.AnswerKeys[0], this.answer1Key);
                        continue;
                    }

                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }

        [Test]
        public void AREventRaisingOnAnswerSetWithPropagation()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);

            using (var ctx = new EventContext())
            {
                completeQuestionnaire.SetAnswer(this.autoPropQuestionKey, null, "1", null, DateTime.UtcNow);

                Assert.True(ctx.Events.Any());

                foreach (var item in ctx.Events)
                {
                    var answerSetEvent = item.Payload as AnswerSet;
                    if (answerSetEvent != null)
                    {
                        Assert.AreEqual(answerSetEvent.QuestionPublicKey, this.autoPropQuestionKey);
                        Assert.AreEqual(answerSetEvent.AnswerKeys, null);
                        Assert.AreEqual(answerSetEvent.AnswerValue, "1");
                        
                        //Assert.AreEqual(answerSetEvent.AnswerKeys[0], this.answer1Key);
                        continue;
                    }

                    var propagatableEvent = item.Payload as PropagatableGroupAdded;
                    if (propagatableEvent != null)
                    {
                        Assert.AreEqual(propagatableEvent.ParentKey, this.mainGroupKey);
                        Assert.AreEqual(propagatableEvent.ParentPropagationKey, null);
                        Assert.AreEqual(propagatableEvent.PublicKey, this.autoGroupKey);
                        
                        continue;
                    }

                    var statusEvent = item.Payload as ConditionalStatusChanged;
                    if (statusEvent != null)
                    {
                        //Assert.AreEqual(statusEvent.CompletedQuestionnaireId, this.mainGroupKey);

                        Assert.AreEqual(statusEvent.ResultGroupsStatus.Count, 1);
                        Assert.AreEqual(statusEvent.ResultQuestionsStatus.Count, 1);

                        continue;
                    }



                    Assert.Fail("Unexpected event was raised.");
                }
            }
        }


        [Test]
        public void AREventRaisingOnChangeAssignment()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);

            using (var ctx = new EventContext())
            {
                var userId = Guid.NewGuid();
                var userName = "test";

                completeQuestionnaire.ChangeAssignment(new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (var item in ctx.Events)
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
        public void AREventRaisingOnStatusChange()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);

            using (var ctx = new EventContext())
            {
                var userId = Guid.NewGuid();
                var userName = "testUser";
                
                completeQuestionnaire.ChangeStatus(SurveyStatus.Initial, new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (var item in ctx.Events)
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
        /// The ar event raising on status change.
        /// </summary>
        [Test]
        public void AREventRaisingOnCommentSet()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);

            using (var ctx = new EventContext())
            {
                var commentText = "test comment";

                completeQuestionnaire.SetComment(this.questionKey, commentText, null);

                Assert.True(ctx.Events.Count() == 1);

                foreach (var item in ctx.Events)
                {
                    var evnt = item.Payload as CommentSet;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.QuestionPublickey, questionKey);
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

            var userId = Guid.NewGuid();
            var userName = "testUser";


            using (var ctx = new EventContext())
            {
                var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, new UserLight(userId, userName));

                Assert.True(ctx.Events.Count() == 1);

                foreach (var item in ctx.Events)
                {
                    var evnt = item.Payload as NewCompleteQuestionnaireCreated;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.Questionnaire.PublicKey, key);
                        Assert.AreEqual(evnt.Questionnaire.Status, SurveyStatus.Unassign);
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

            var userId = Guid.NewGuid();
            var userName = "testUser";

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, new UserLight(userId, userName));

            using (var ctx = new EventContext())
            {
                completeQuestionnaire.Delete();

                Assert.True(ctx.Events.Count() == 1);

                foreach (var item in ctx.Events)
                {
                    var evnt = item.Payload as CompleteQuestionnaireDeleted;
                    if (evnt != null)
                    {
                        Assert.AreEqual(evnt.CompletedQuestionnaireId, key);
                        Assert.AreEqual(evnt.TemplateId, this.document.PublicKey);
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
            var doc = new QuestionnaireDocument();
            var mainGroup = new Group("Main") { PublicKey = mainGroupKey };
            var group1 = new Group();
            var question1 = new SingleQuestion(this.questionKey, "Q1");
            var answer1 = new Answer { PublicKey = this.answer1Key, AnswerValue = "1" };
            var answer2 = new Answer { PublicKey = this.answer2Key, AnswerValue = "2" };


            var question2prop = new AutoPropagateQuestion("Q1") { PublicKey = this.autoPropQuestionKey, MaxValue = 10};
            
            question2prop.Triggers = new List<Guid>() { this.autoGroupKey };


            question1.AddAnswer(answer1);
            question1.AddAnswer(answer2);
            group1.Children.Add(question1);
            group1.Children.Add(question2prop);


            var group2auto = new Group() { Propagated = Propagate.AutoPropagated, PublicKey = this.autoGroupKey };
            
            var autoQuestion1 = new SingleQuestion(this.autoQuestionKey, "Q1a");
            var autoAnswer1 = new Answer { PublicKey = this.autoAnswer1Key, AnswerValue = "1" };
            var autoAnswer2 = new Answer { PublicKey = this.autoAnswer2Key, AnswerValue = "2" };

            autoQuestion1.AddAnswer(autoAnswer1);
            autoQuestion1.AddAnswer(autoAnswer2);
            group2auto.Children.Add(autoQuestion1);

            mainGroup.Children.Add(group1);
            mainGroup.Children.Add(group2auto);
            doc.Add(mainGroup, null, null);

            this.document = doc;
        }

        #endregion
    }
}