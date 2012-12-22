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

    using Ncqrs.Domain;
    using Ncqrs.Eventing;

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
        /// The document.
        /// </summary>
        private QuestionnaireDocument document;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The event raising.
        /// </summary>
        [Test]
        public void AREventRaising()
        {
            Guid key = Guid.NewGuid();
            Guid commandId = Guid.NewGuid();

            var completeQuestionnaire = new CompleteQuestionnaireAR(key, this.document, null);
            var _eventStream = new UncommittedEventStream(commandId);
            AggregateRoot.RegisterThreadStaticEventAppliedCallback(
                (aggregateRoot, UncommittedEvent) => { _eventStream.Append(UncommittedEvent); });

            completeQuestionnaire.SetAnswer(
                this.questionKey, null, null, new List<Guid> { this.answer1Key }, DateTime.UtcNow);
            Assert.True(_eventStream.Any());

            foreach (var item in _eventStream)
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

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            var doc = new QuestionnaireDocument();
            var mainGroup = new Group();
            var group1 = new Group();
            var question1 = new SingleQuestion(this.questionKey, "Q1");
            var answer1 = new Answer { PublicKey = this.answer1Key, AnswerValue = "1" };
            var answer2 = new Answer { PublicKey = this.answer2Key, AnswerValue = "2" };

            question1.AddAnswer(answer1);
            question1.AddAnswer(answer2);
            group1.Children.Add(question1);
            mainGroup.Children.Add(group1);
            doc.Add(mainGroup, null, null);

            this.document = doc;
        }

        #endregion
    }
}