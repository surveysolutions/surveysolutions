using System;
using NUnit.Framework;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.Tests.Entities.SubEntities
{
    [TestFixture]
    public class SingleCompleteQuestionTest
    {
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            SingleCompleteQuestion completeQuestion = new SingleCompleteQuestion()
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
            completeQuestion.Triggers = new List<Guid>() { Guid.NewGuid() };
            List<IComposite> children = new List<IComposite>() { new Answer() {AnswerText = "some text"}, new Answer(), new Answer() };
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
            Assert.AreEqual(((Answer)(completeQuestion.Children[0])).AnswerText, "some text");
        }

        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var completeAnswer = new CompleteAnswer() {AnswerValue = 5, AnswerText = "5", Selected = true};
            var question = new SingleCompleteQuestion { Children = new List<IComposite>() { completeAnswer, new CompleteAnswer() } };
            var completeQuestion = question;
            Assert.AreEqual(question.Children.Count, completeQuestion.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = completeQuestion.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsNotNull(answer);
            }
            Assert.AreEqual(question.Answer, completeAnswer);
        }

        [Test]
        public void CheckAdded_Answers()
        {
            var completeAnswer = new CompleteAnswer() { AnswerValue = 5, AnswerText = "5", Selected = true };
            var newSelectedAnswer = new CompleteAnswer() { AnswerValue = 56, AnswerText = "56" };
            var question = new SingleCompleteQuestion { Children = new List<IComposite>() { completeAnswer, newSelectedAnswer } };
            question.Add(newSelectedAnswer, question.PublicKey);
            Assert.AreEqual(question.Answer, newSelectedAnswer);
        }
    }
}
