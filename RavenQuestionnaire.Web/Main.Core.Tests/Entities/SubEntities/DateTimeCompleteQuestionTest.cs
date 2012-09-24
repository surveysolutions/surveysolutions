// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeCompleteQuestionTest.cs" company="">
//   
// </copyright>
// <summary>
//   The date time complete question test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Tests.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;

    using NUnit.Framework;

    /// <summary>
    /// The date time complete question test.
    /// </summary>
    [TestFixture]
    public class DateTimeCompleteQuestionTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The check conversion_ answers are converted.
        /// </summary>
        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var answers = new List<IComposite> { new CompleteAnswer() };
            var question = new DateTimeCompleteQuestion { Children = answers };
            var completeQuestion = question;
            Assert.AreEqual(question.Children.Count, completeQuestion.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = completeQuestion.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsTrue(answer != null);
            }
        }

        /// <summary>
        /// The when set main settings_ is added.
        /// </summary>
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            var completeQuestion = new DateTimeCompleteQuestion();
            completeQuestion.AnswerDate = DateTime.Today;
            completeQuestion.AnswerOrder = Order.MaxMin;
            completeQuestion.Cards.Add(new Image { Description = "some image", Title = "some image" });
            completeQuestion.ConditionExpression = "some expression";
            completeQuestion.Instructions = "some instructions";
            completeQuestion.QuestionText = "question text";
            completeQuestion.QuestionType = QuestionType.DateTime;
            completeQuestion.StataExportCaption = "some stata export caption";
            completeQuestion.ValidationExpression = "some validation expression";
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.Cards.Count, 1);
            Assert.AreEqual(completeQuestion.Cards[0].Title, "some image");
            Assert.AreEqual(completeQuestion.Cards[0].Description, "some image");
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.DateTime);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
        }

        #endregion
    }
}