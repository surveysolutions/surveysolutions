// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtentionFunctionsTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ExtentionFunctionsTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Tests.ExpressionExecutors.ExpressionExtentions
{
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The extention functions tests.
    /// </summary>
    [TestFixture]
    public class ExtentionFunctionsTests
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
            question.Children.Add(answer);
            question.Children.Add(answer1);
            question.Children.Add(answer2);

            var executorC = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            bool? result = executorC.Execute(question);
            Assert.AreEqual(result, false);

            var executorE = new CompleteQuestionnaireValidationExecutor(new GroupHash(doc));
            bool? result1 = executorE.Execute(question);
            Assert.AreEqual(result1, true);
        }
    }
}
