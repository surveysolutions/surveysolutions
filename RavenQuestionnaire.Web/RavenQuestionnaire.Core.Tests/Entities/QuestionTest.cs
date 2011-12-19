using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class QuestionTest
    {
        [Test]
        public void WhenSetConditionExpression_ExpressionIsAdded()
        {
            Question question = new Question();
            question.SetConditionExpression("some expression");
            Assert.AreEqual(question.ConditionExpression, "some expression");
        }
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            Question question = new Question();
            bool result = question.EvaluateCondition(new List<CompleteAnswer>());
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            Question question = new Question();
            question.SetConditionExpression("invalid condition");
            bool result = question.EvaluateCondition(new List<CompleteAnswer>());
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            Question question = new Question();
            question.SetConditionExpression("5>3");
            bool result = question.EvaluateCondition(new List<CompleteAnswer>());
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

            Question question = new Question();
            question.SetConditionExpression("[" + completeAnswer.QuestionPublicKey + "]==3");
            
            
            bool result = question.EvaluateCondition(previousResults);
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

            Question question = new Question();
            question.SetConditionExpression("[" + answer.QuestionPublicKey + "]==3");


            bool result = question.EvaluateCondition(previousResults);
            Assert.AreEqual(result, false);
        }
    }
}
