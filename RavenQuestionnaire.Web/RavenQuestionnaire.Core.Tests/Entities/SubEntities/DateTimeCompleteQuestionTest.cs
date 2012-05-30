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
    public class DateTimeCompleteQuestionTest
    {
        [Test]
        public void WhenSetMainSettings_IsAdded()
        {
            DateTimeCompleteQuestion completeQuestion = new DateTimeCompleteQuestion();
            completeQuestion.AnswerDate = DateTime.Today;
            completeQuestion.AnswerOrder = Order.MaxMin;
            completeQuestion.Attributes.Add("Lenght", "12");
            completeQuestion.Attributes.Add("Size", "10px");
            completeQuestion.Cards.Add(new Image(){Description = "some image", Title = "some image"});
            completeQuestion.ConditionExpression = "some expression";
            completeQuestion.Instructions = "some instructions";
            completeQuestion.QuestionText = "question text";
            completeQuestion.QuestionType = QuestionType.DateTime;
            completeQuestion.StataExportCaption = "some stata export caption";
            completeQuestion.ValidationExpression = "some validation expression";
            completeQuestion.Triggers = new List<Guid>() { Guid.NewGuid() };
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.Attributes.Count, 2);
            Assert.IsTrue(completeQuestion.Attributes.ContainsKey("Lenght"));
            Assert.IsTrue(completeQuestion.Attributes.ContainsKey("Size"));
            Assert.IsTrue(completeQuestion.Attributes.ContainsValue("12"));
            Assert.IsTrue(completeQuestion.Attributes.ContainsValue("10px"));
            Assert.AreEqual(completeQuestion.Cards.Count, 1);
            Assert.AreEqual(completeQuestion.Cards[0].Title, "some image");
            Assert.AreEqual(completeQuestion.Cards[0].Description, "some image");
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.DateTime);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
            Assert.AreEqual(completeQuestion.Triggers.Count, 1);
        }

        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var answers = new List<IComposite>() { new CompleteAnswer() };
            var question = new DateTimeCompleteQuestion { Children = answers };
            var completeQuestion = question;
            Assert.AreEqual(question.Children.Count, completeQuestion.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = completeQuestion.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsTrue(answer != null);
            }
        }
    }
}
