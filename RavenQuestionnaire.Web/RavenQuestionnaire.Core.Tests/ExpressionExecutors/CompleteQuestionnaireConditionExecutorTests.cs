using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    [TestFixture]
    public class CompleteQuestionnaireConditionExecutorTests
    {
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor= new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            bool result = executor.Execute("");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            bool result = executor.Execute("invalid condition");
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());

            bool result = executor.Execute("5>3");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            var completeAnswer = new CompleteAnswer();
            completeAnswer.QuestionPublicKey = Guid.NewGuid();
            completeAnswer.CustomAnswer = "3";
            var previousResults = new List<CompleteAnswer>();
            previousResults.Add(completeAnswer);

            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument() { CompletedAnswers = previousResults });

            bool result = executor.Execute("[" + completeAnswer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInValidParamsAreNotEmpty_ReturnsTrue()
        {
            var answer = new CompleteAnswer();
            answer.QuestionPublicKey = Guid.NewGuid();
            answer.CustomAnswer = "invalid value";
            var previousResults = new List<CompleteAnswer>();
            previousResults.Add(answer);

            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument() { CompletedAnswers = previousResults });

            bool result = executor.Execute("[" + answer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, false);
        }
    }
}
