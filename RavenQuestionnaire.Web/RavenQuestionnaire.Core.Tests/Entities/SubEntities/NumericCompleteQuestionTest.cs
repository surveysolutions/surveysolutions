using System;
using System.Collections.Generic;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.Tests.Entities.SubEntities
{
    [TestFixture]
    public class NumericCompleteQuestionTest
    {
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            NumericCompleteQuestion completeQuestion = new NumericCompleteQuestion()
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
            completeQuestion.Triggers = new List<Guid>() { Guid.NewGuid() };
            List<IComposite> children = new List<IComposite>() { new Answer() };
            completeQuestion.Children = children;
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.Numeric);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
            Assert.AreEqual(completeQuestion.Triggers.Count, 1);
        }

        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var question = new NumericCompleteQuestion { Children = new List<IComposite>() { new CompleteAnswer(){AnswerValue = 5, AnswerText = "5"}} };
            var completeQuestion = question;
            Assert.AreEqual(question.Children.Count, completeQuestion.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = completeQuestion.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsNotNull(answer);
                Assert.AreEqual(question.Answer, answer.AnswerValue);
            }
        }
    }
}
