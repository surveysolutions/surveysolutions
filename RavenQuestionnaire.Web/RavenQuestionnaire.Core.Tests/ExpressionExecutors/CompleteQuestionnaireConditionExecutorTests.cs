using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    [TestFixture]
    public class CompleteQuestionnaireConditionExecutorTests
    {
      //  public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
           // iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            bool result = executor.Execute(
                                           new CompleteQuestion());
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            bool result = executor.Execute(
                                           new CompleteQuestion() {ConditionExpression = "invalid condition"});
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());

            bool result = executor.Execute(
                                           new CompleteQuestion() {ConditionExpression = "5>3"});
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire entity = new CompleteQuestionnaire(doc);
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(doc);
            var question = new CompleteQuestion("", QuestionType.SingleOption);
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);

            var completeAnswer = new CompleteAnswer(new Answer(), doc.Children[0].PublicKey);
            completeAnswer.AnswerType = AnswerType.Select;
            completeAnswer.AnswerValue = "3";
            completeAnswer.Selected = true;
            question.Children.Add(completeAnswer);
            bool result = executor.Execute(question);
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInValidParamsAreNotEmpty_ReturnsTrue()
        {
            var answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            answer.AnswerValue = "invalid value";

            
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire entity = new CompleteQuestionnaire(doc);
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(doc);
            var question = new CompleteQuestion("", QuestionType.SingleOption);
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);
            question.Children.Add(answer);
            bool result = executor.Execute(question);
            Assert.AreEqual(result, false);
        }
    }
}
