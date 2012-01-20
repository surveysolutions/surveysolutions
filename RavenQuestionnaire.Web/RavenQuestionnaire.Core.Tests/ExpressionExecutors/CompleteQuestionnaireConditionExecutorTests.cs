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
        public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
            iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor= new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new List<CompleteAnswer>(), "");
            Assert.AreEqual(result, true);
        }
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsFalse()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();
            bool result = executor.Execute(new List<CompleteAnswer>(), "invalid condition");
            Assert.AreEqual(result, false);
        }
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            CompleteQuestionnaireConditionExecutor executor = new CompleteQuestionnaireConditionExecutor();

            bool result = executor.Execute(new List<CompleteAnswer>(), "5>3");
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
            iteratorContainerMock.Setup(
            x => x.Create<CompleteQuestionnaireDocument, CompleteQuestion>(doc)).Returns(
                new QuestionnaireQuestionIterator(doc));
            iteratorContainerMock.Setup(
             x => x.Create<CompleteQuestionnaireDocument, CompleteAnswer>(doc)).Returns(
                 new QuestionnaireAnswerIterator(doc));
            bool result = executor.Execute(doc.Questions[0].Answers, "[" + completeAnswer.QuestionPublicKey + "]==3");
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
            iteratorContainerMock.Setup(
            x => x.Create<CompleteQuestionnaireDocument, CompleteQuestion>(doc)).Returns(
                new QuestionnaireQuestionIterator(doc));
            iteratorContainerMock.Setup(
             x => x.Create<CompleteQuestionnaireDocument, CompleteAnswer>(doc)).Returns(
                 new QuestionnaireAnswerIterator(doc));
            bool result = executor.Execute(doc.Questions[0].Answers,
                                           "[" + answer.QuestionPublicKey + "]==3");
            Assert.AreEqual(result, false);
        }
    }
}
