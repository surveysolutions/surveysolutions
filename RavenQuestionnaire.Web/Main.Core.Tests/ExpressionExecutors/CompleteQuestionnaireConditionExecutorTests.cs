namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The complete questionnaire condition executor tests.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireConditionExecutorTests
    {
        // public Mock<IIteratorContainer> iteratorContainerMock;
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            // iteratorContainerMock = new Mock<IIteratorContainer>();
        }

        /// <summary>
        /// The evaluate condition_ condition is empty_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsEmpty_ReturnsTrue()
        {
            var executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());
            bool? result = executor.Execute(new SingleCompleteQuestion());
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ condition is in valid params are not empty_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsInValidParamsAreNotEmpty_ReturnsTrue()
        {
            var answer = new CompleteAnswer(new Answer());
            answer.AnswerValue = "invalid value";

            var doc = new CompleteQuestionnaireDocument();

            var question = new SingleCompleteQuestion(string.Empty);
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);
            question.AddAnswer(answer);
            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            bool? result = executor.Execute(question);
            Assert.AreEqual(result, false);
        }

        /// <summary>
        /// The evaluate condition_ condition is invalid_ returns null.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsInvalid_ReturnsNull()
        {
            var executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireStoreDocument());
            bool? result = executor.Execute(new SingleCompleteQuestion { ConditionExpression = "invalid condition" });
            Assert.AreEqual(result, null);
        }

        /// <summary>
        /// The evaluate condition_ condition is invalid_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsAbsent_ReturnsTrue()
        {
            var executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireStoreDocument());
            bool? result = executor.Execute(new SingleCompleteQuestion { ConditionExpression = string.Empty });
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ condition is valid params are empty_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreEmpty_ReturnsTrue()
        {
            var executor = new CompleteQuestionnaireConditionExecutor(new CompleteQuestionnaireDocument());

            bool? result = executor.Execute(new SingleCompleteQuestion { ConditionExpression = "5>3" });
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ condition is valid params are not empty_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            var doc = new CompleteQuestionnaireDocument();
            var question = new SingleCompleteQuestion(string.Empty);
            question.ConditionExpression = "[" + question.PublicKey + "]==3";
            doc.Children.Add(question);

            var completeAnswer = new CompleteAnswer(new Answer());
            completeAnswer.AnswerType = AnswerType.Select;
            completeAnswer.AnswerValue = "3";
            completeAnswer.Selected = true;
            question.Answers.Add(completeAnswer);
            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            bool? result = executor.Execute(question);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ condition is valid params are not empty_ returns false.
        /// </summary>
        [Test]
        public void EvaluateCondition_ConditionIsNotValidParamsAreNotEmpty_ReturnsFalse()
        {
            var doc = new CompleteQuestionnaireDocument();
            var question = new SingleCompleteQuestion(string.Empty);
            var question1 = new SingleCompleteQuestion(string.Empty);
            question1.ConditionExpression = "[" + question.PublicKey + "]==1";
            doc.Children.Add(question);
            doc.Children.Add(question1);

            var completeAnswer = new CompleteAnswer(new Answer());
            completeAnswer.AnswerType = AnswerType.Select;
            completeAnswer.AnswerValue = "3";
            completeAnswer.Selected = true;
            question.Answers.Add(completeAnswer);
            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            bool? result = executor.Execute(question1);
            Assert.AreEqual(result, false);
        }

        /// <summary>
        /// The evaluate condition_ condition is valid params are not empty_ returns true.
        /// </summary>
        [Test]
        public void EvaluateCondition_GroupConditionWithNestedIsValidParamsAreNotEmpty_ReturnsTrue()
        {
            var doc = new CompleteQuestionnaireDocument();
            var question = new SingleCompleteQuestion(string.Empty);
            var question1 = new SingleCompleteQuestion(string.Empty);
            var group = new CompleteGroup();
            group.ConditionExpression = "[" + question.PublicKey + "]==3";
            question.ConditionExpression = "[" + question1.PublicKey + "]==1";

            doc.Children.Add(group);
            doc.Children.Add(question);
            doc.Children.Add(question1);

            var completeAnswer = new CompleteAnswer(new Answer());
            completeAnswer.AnswerType = AnswerType.Select;
            completeAnswer.AnswerValue = "3";
            completeAnswer.Selected = true;
            question.Answers.Add(completeAnswer);

            var completeAnswer1 = new CompleteAnswer(new Answer());
            completeAnswer1.AnswerType = AnswerType.Select;
            completeAnswer1.AnswerValue = "1";
            completeAnswer1.Selected = true;
            question1.Answers.Add(completeAnswer1);

            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            bool? result = executor.Execute(group);
            Assert.AreEqual(result, true);
        }

        #endregion
    }
}