namespace Main.Core.Tests.ExpressionExecutors.ExpressionExtentions
{
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The extension functions tests.
    /// </summary>
    [TestFixture]
    public class ExtensionFunctionsTests
    {
        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
        }

        /// <summary>
        /// The expression should evaluate contain functions with parameters.
        /// </summary>
        [Test]
        public void ExpressionShouldEvaluateContainFunctionsWithParameters()
        {
            var answer = new CompleteAnswer(new Answer() { });
            answer.AnswerValue = "1";
            
            var answer1 = new CompleteAnswer(new Answer() { });
            answer1.AnswerValue = "2";
            answer1.Selected = true;

            var answer2 = new CompleteAnswer(new Answer() { });
            answer2.AnswerValue = "3";
            answer2.Selected = true;

            var doc = new CompleteQuestionnaireDocument();
            var question = new MultyOptionsCompleteQuestion(string.Empty);

            question.ConditionExpression = "contains([" + question.PublicKey + "],1)";
            question.ValidationExpression = "contains([" + question.PublicKey + "],3)" + " and " + "contains([" + question.PublicKey + "],2)";
            
            doc.Children.Add(question);
            question.AddAnswer(answer);
            question.AddAnswer(answer1);
            question.AddAnswer(answer2);

            var executorC = new CompleteQuestionnaireConditionExecutor(doc);
            bool? result = executorC.Execute(question);
            Assert.AreEqual(result, false);

            var executorE = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            bool? result1 = executorE.Execute(question);
            Assert.AreEqual(result1, true);
        }
    }
}
