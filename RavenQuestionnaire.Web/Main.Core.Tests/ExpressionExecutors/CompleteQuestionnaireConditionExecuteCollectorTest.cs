using Microsoft.Practices.ServiceLocation;
using Moq;

namespace Main.Core.Tests.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The complete questionnaire condition execute collector test.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireConditionExecuteCollectorTest
    {
        /// <summary>
        /// The q 1 public key.
        /// </summary>
        private readonly Guid q1PublicKey = Guid.NewGuid();

        /// <summary>
        /// The q 2_ public key.
        /// </summary>
        private readonly Guid q2PublicKey = Guid.NewGuid();

        /// <summary>
        /// The public key.
        /// </summary>
        private readonly Guid q21PublicKey = Guid.NewGuid();

        /// <summary>
        /// The public key.
        /// </summary>
        private readonly Guid g1PublicKey = Guid.NewGuid();

        /// <summary>
        /// The public key.
        /// </summary>
        private readonly Guid g2PublicKey = Guid.NewGuid();

        /// <summary>
        /// The doc.
        /// </summary>
        private QuestionnaireDocument doc;

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
	        ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            
            doc = new QuestionnaireDocument();

            var mainGroup = new Group("Main Group");

            var question1 = new NumericQuestion()
            {
                PublicKey = this.q1PublicKey
            };

            var question2 = new NumericQuestion
            {
                ConditionExpression = string.Format("[{0}] > 3", question1.PublicKey),
                PublicKey = this.q2PublicKey
            };

            var subGroup1 = new Group("Sub Group1")
                {
                    ConditionExpression = string.Format("[{0}] > 1", question1.PublicKey),
                    PublicKey = this.g1PublicKey
                };

            var subGroup2 = new Group("Sub Group2")
            {
                ConditionExpression =
                    string.Format("[{0}] > 3 and [{1}] != 1", question1.PublicKey, question2.PublicKey),
                    PublicKey = this.g2PublicKey
            };

            var question21 = new NumericQuestion
            {
                ConditionExpression = string.Format("[{0}] > 3", question1.PublicKey),
                PublicKey = this.q21PublicKey
            };

            subGroup2.Children.Add(question21);
            mainGroup.Children.Add(question1);
            mainGroup.Children.Add(question2);
            mainGroup.Children.Add(subGroup1);
            mainGroup.Children.Add(subGroup2);

            this.doc.Add(mainGroup, null, null);
        }

        /// <summary>
        /// The collect changes test.
        /// </summary>
        [Test]
        public void CollectChangesTest()
        {
            var completedQ = (CompleteQuestionnaireDocument)this.doc;

            var q = completedQ.GetQuestion(this.q1PublicKey, null);
            q.SetAnswer(null, "2");

            var resultQuestionsStatus = new Dictionary<string, bool?>();
            var resultGroupsStatus = new Dictionary<string, bool?>();

            var collector = new CompleteQuestionnaireConditionExecuteCollector(completedQ);

            collector.ExecuteConditionAfterAnswer(q, resultQuestionsStatus, resultGroupsStatus);

            Assert.AreEqual(resultQuestionsStatus.Count, 2);
            Assert.AreEqual(resultGroupsStatus.Count, 2);

            Assert.AreEqual(resultQuestionsStatus[this.q2PublicKey.ToString()].Value, false);
            Assert.AreEqual(resultGroupsStatus[this.g2PublicKey.ToString()].Value, false);

            resultQuestionsStatus.Clear();
            resultGroupsStatus.Clear();
            q.SetAnswer(null, "4");

            collector.ExecuteConditionAfterAnswer(q, resultQuestionsStatus, resultGroupsStatus);

            Assert.AreEqual(resultQuestionsStatus.Count > 0, true);
            Assert.AreEqual(resultGroupsStatus.Count > 0, true);

            Assert.AreEqual(resultQuestionsStatus[this.q2PublicKey.ToString()].Value, true);
            Assert.AreEqual(resultGroupsStatus[this.g2PublicKey.ToString()].Value, true);

        }


        /// <summary>
        /// The collect changes test.
        /// </summary>
        [Test]
        public void EmptyConditions_Produce_Empty_Results()
        {
            doc = new QuestionnaireDocument();

            var mainGroup = new Group("Main Group");

            var question1 = new NumericQuestion()
            {
                PublicKey = this.q1PublicKey
            };

            var question2 = new NumericQuestion
            {
                ConditionExpression = "3 = 3",
                PublicKey = this.q2PublicKey
            };

            var question3 = new NumericQuestion
            {
                ConditionExpression = string.Format("'[{0}]' = '[{0}]'", question1.PublicKey),
                PublicKey = Guid.NewGuid()
            };

            mainGroup.Children.Add(question1);
            mainGroup.Children.Add(question2);
            mainGroup.Children.Add(question3);
            doc.Add(mainGroup, null, null);

            var completedQ = (CompleteQuestionnaireDocument)this.doc;

            var q = completedQ.GetQuestion(this.q1PublicKey, null);
            q.SetAnswer(null, "2");

            var resultQuestionsStatus = new Dictionary<string, bool?>();
            var resultGroupsStatus = new Dictionary<string, bool?>();

            var collector = new CompleteQuestionnaireConditionExecuteCollector(completedQ);

            collector.ExecuteConditionAfterAnswer(q, resultQuestionsStatus, resultGroupsStatus);

            Assert.AreEqual(resultQuestionsStatus.Count, 0);
            Assert.AreEqual(resultGroupsStatus.Count, 0);

        }
    }
}
