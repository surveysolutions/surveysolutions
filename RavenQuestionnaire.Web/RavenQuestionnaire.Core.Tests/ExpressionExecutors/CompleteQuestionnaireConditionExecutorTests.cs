using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    [TestFixture]
    public class CompleteQuestionnaireConditionExecutorTests
    {
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaireConditionExecutor executor= new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new CompleteQuestionnaire(doc), "");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new CompleteQuestionnaire(doc), "invalid condition");
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();

            bool result = executor.Execute(new CompleteQuestionnaire(doc), "5>3");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            doc.Questions.Add(new CompleteQuestion("", QuestionType.SingleOption));

            var completeAnswer = new CompleteAnswer(new Answer(), doc.Questions[0].PublicKey);
            completeAnswer.AnswerType = AnswerType.Text;
            completeAnswer.CustomAnswer = "3";
            completeAnswer.Selected = true;
            doc.Questions[0].Answers.Add(completeAnswer);
            bool result = executor.Execute(new CompleteQuestionnaire(doc), "[" + completeAnswer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInValidParamsAreNotEmpty_ReturnsTrue()
        {
            var answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            answer.CustomAnswer = "invalid value";

            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();
            CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();
            doc.Questions.Add(new CompleteQuestion("", QuestionType.SingleOption));
            doc.Questions[0].Answers.Add(answer);
            bool result = executor.Execute(new CompleteQuestionnaire(doc),
                                           "[" + answer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, false);
        }
    }
}
