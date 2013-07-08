namespace RavenQuestionnaire.Core.Tests.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;

    using NUnit.Framework;

    /// <summary>
    /// The numeric complete question test.
    /// </summary>
    [TestFixture]
    public class NumericCompleteQuestionTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The check conversion_ answers are converted.
        /// </summary>
        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var question = new NumericCompleteQuestion();
            question.SetAnswer(null, "5");
                
            var completeQuestion = question;
            Assert.AreEqual(question.Answers.Count, completeQuestion.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = completeQuestion.Answers.FirstOrDefault(q => q.PublicKey == question.Answers[i].PublicKey);
                Assert.IsNotNull(answer);
                Assert.AreEqual(question.GetAnswerObject(), answer.AnswerValue);
            }
        }


        /// <summary>
        /// The check conversion_ answers are converted.
        /// </summary>
        [Test]
        public void CheckConversion_NullSupport()
        {
            var question = new NumericCompleteQuestion();
            question.SetAnswer(null, "5");

            Assert.AreEqual(question.GetAnswerObject(), 5);

            question.SetAnswer(null, "");
            Assert.AreEqual(question.GetAnswerObject(), null);
        }

        /// <summary>
        /// The when set main settings_ is added.
        /// </summary>
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            var completeQuestion = new NumericCompleteQuestion
                {
                    AnswerDate = DateTime.Today, 
                    AnswerOrder = Order.MaxMin, 
                    QuestionType = QuestionType.Numeric, 
                    ConditionExpression = "some expression", 
                    Instructions = "some instructions", 
                    QuestionText = "question text", 
                    StataExportCaption = "some stata export caption", 
                    ValidationExpression = "some validation expression"
                };
            var children = new List<IAnswer> { new Answer() };
            completeQuestion.Answers = children;
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.Numeric);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
        }

        #endregion
    }
}