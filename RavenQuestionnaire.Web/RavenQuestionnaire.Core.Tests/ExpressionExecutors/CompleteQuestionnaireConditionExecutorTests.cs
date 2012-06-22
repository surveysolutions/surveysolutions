using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
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
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new GroupHash());
            bool result = executor.Execute(
                                           new SingleCompleteQuestion());
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new GroupHash());
            bool result = executor.Execute(
                                           new SingleCompleteQuestion() { ConditionExpression = "invalid condition" });
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new GroupHash());

            bool result = executor.Execute(
                                           new SingleCompleteQuestion() { ConditionExpression = "5>3" });
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire entity = new CompleteQuestionnaire(doc);
           
            var question = new SingleCompleteQuestion("");
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);

            var completeAnswer = new CompleteAnswer(new Answer());
            completeAnswer.AnswerType = AnswerType.Select;
            completeAnswer.AnswerValue = "3";
            completeAnswer.Selected = true;
            question.Children.Add(completeAnswer);
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            bool result = executor.Execute(question);
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInValidParamsAreNotEmpty_ReturnsTrue()
        {
            var answer = new CompleteAnswer(new Answer());
            answer.AnswerValue = "invalid value";

            
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaire entity = new CompleteQuestionnaire(doc);
           
            var question = new SingleCompleteQuestion("");
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);
            question.Children.Add(answer);
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            bool result = executor.Execute(question);
            Assert.AreEqual(result, false);
        }
    }
}
