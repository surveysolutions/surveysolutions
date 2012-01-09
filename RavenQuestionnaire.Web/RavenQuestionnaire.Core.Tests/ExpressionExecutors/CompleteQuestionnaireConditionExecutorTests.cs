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
            CompleteQuestionnaireConditionExecutor executor= new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new CompleteQuestionnaireDocument(),"");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new CompleteQuestionnaireDocument(), "invalid condition");
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();

            bool result = executor.Execute(new CompleteQuestionnaireDocument(), "5>3");
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

            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();

            bool result = executor.Execute(new CompleteQuestionnaireDocument() { CompletedAnswers = previousResults }, "[" + completeAnswer.QuestionPublicKey + "]==3");
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

            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();

            bool result = executor.Execute(new CompleteQuestionnaireDocument() {CompletedAnswers = previousResults},
                                           "[" + answer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, false);
        }
    }
}
