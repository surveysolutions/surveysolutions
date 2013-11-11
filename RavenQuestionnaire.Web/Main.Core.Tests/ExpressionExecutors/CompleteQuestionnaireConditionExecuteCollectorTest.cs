using Microsoft.Practices.ServiceLocation;
using Moq;

namespace Main.Core.Tests.ExpressionExecutors
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
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
    }
}
