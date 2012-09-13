// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleCompleteQuestionTest.cs" company="">
//   
// </copyright>
// <summary>
//   The single complete question test.
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
    /// The single complete question test.
    /// </summary>
    [TestFixture]
    public class SingleCompleteQuestionTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The check conversion_ answers are converted.
        /// </summary>
        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var completeAnswer = new CompleteAnswer { AnswerValue = 5, AnswerText = "5", Selected = true };
            var question = new SingleCompleteQuestion
                {
                   Children = new List<IComposite> { completeAnswer, new CompleteAnswer() } 
                };
            var completeQuestion = question;
            Assert.AreEqual(question.Children.Count, completeQuestion.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = completeQuestion.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsNotNull(answer);
            }

            Assert.AreEqual(question.GetAnswerObject(), completeAnswer.AnswerValue);
        }

        /// <summary>
        /// The when set main settings_ is added.
        /// </summary>
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            var completeQuestion = new SingleCompleteQuestion
                {
                    AnswerDate = DateTime.Today, 
                    AnswerOrder = Order.MaxMin, 
                    QuestionType = QuestionType.SingleOption, 
                    ConditionExpression = "some expression", 
                    Instructions = "some instructions", 
                    QuestionText = "question text", 
                    StataExportCaption = "some stata export caption", 
                    ValidationExpression = "some validation expression"
                };
            completeQuestion.Triggers = new List<Guid> { Guid.NewGuid() };
            var children = new List<IComposite> { new Answer { AnswerText = "some text" }, new Answer(), new Answer() };
            completeQuestion.Children = children;
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.SingleOption);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
            Assert.AreEqual(completeQuestion.Triggers.Count, 1);
            Assert.AreEqual(completeQuestion.Children.Count, 3);
            Assert.AreEqual(((Answer)completeQuestion.Children[0]).AnswerText, "some text");
        }

        #endregion

        /*[Test]
        public void CheckAdded_Answers()
        {
            var completeAnswer = new CompleteAnswer() { AnswerValue = 5, AnswerText = "5", Selected = true };
            var newSelectedAnswer = new CompleteAnswer() { AnswerValue = 56, AnswerText = "56" };
            var question = new SingleCompleteQuestion { Children = new List<IComposite>() { completeAnswer, newSelectedAnswer } };
            question.Add(newSelectedAnswer, question.PublicKey);
            Assert.AreEqual(question.GetAnswerObject(), newSelectedAnswer.AnswerValue);
        }*/
    }
}