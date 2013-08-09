using Microsoft.Practices.ServiceLocation;
using Moq;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The questionnaire parameters parser test.
    /// </summary>
    [TestFixture]
    public class QuestionnaireParametersParserTest
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        #region Public Methods and Operators

        /// <summary>
        /// The parse condition_ condition is empty_ returns zero of paremeters.
        /// </summary>
        [Test]
        public void ParseCondition_ConditionIsEmpty_ReturnsZeroOfParemeters()
        {
            var doc = new QuestionnaireDocument();
            var executor = new QuestionnaireParametersParser();
            var result = executor.Execute(doc, string.Empty);
            Assert.AreEqual(result.Count, 0);
        }

        /// <summary>
        /// The parse condition_ condition with 2 parameter_ parametersare returned.
        /// </summary>
        [Test]
        public void ParseCondition_ConditionWith2Parameter_ParametersareReturned()
        {
            var doc = new QuestionnaireDocument();
            var question1 = new SingleQuestion(Guid.NewGuid(), "some1");
            doc.Children.Add(question1);
            var question2 = new SingleQuestion(Guid.NewGuid(), "some2");
            doc.Children.Add(question2);
            var executor = new QuestionnaireParametersParser();
            var result = executor.Execute(
                doc, string.Format("[{0}]==1 and [{1}]>3", question1.PublicKey, question2.PublicKey));
            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0].PublicKey, question1.PublicKey);
            Assert.AreEqual(result[1].PublicKey, question2.PublicKey);
        }

        /// <summary>
        /// The parse condition_ condition with silgle parameter_ parameter is returned.
        /// </summary>
        [Test]
        public void ParseCondition_ConditionWithSilgleParameter_ParameterIsReturned()
        {
            var doc = new QuestionnaireDocument();
            var question = new SingleQuestion(Guid.NewGuid(), "some");
            doc.Children.Add(question);
            var executor = new QuestionnaireParametersParser();
            var result = executor.Execute(doc, string.Format("[{0}]==1", question.PublicKey));
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].PublicKey, question.PublicKey);
        }

        #endregion
    }
}