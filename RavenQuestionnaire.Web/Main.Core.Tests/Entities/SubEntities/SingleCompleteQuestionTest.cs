using Microsoft.Practices.ServiceLocation;
using Moq;

namespace RavenQuestionnaire.Core.Tests.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;

    using NUnit.Framework;

    using RavenQuestionnaire.Core.Tests.Utils;

    /// <summary>
    /// The single complete question test.
    /// </summary>
    [TestFixture]
    public class SingleCompleteQuestionTest
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        #region Public Methods and Operators

        /// <summary>
        /// The check conversion_ answers are converted.
        /// </summary>
        [Test]
        public void CheckConversion_AnswersAreConverted()
        {
            var completeAnswer = new CompleteAnswer { AnswerValue = "5", AnswerText = "5", Selected = true };
            var question = new SingleCompleteQuestion
                {
                   Answers = new List<IAnswer> { completeAnswer, new CompleteAnswer() } 
                };
            var completeQuestion = question;
            Assert.AreEqual(question.Answers.Count, completeQuestion.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = completeQuestion.Answers.FirstOrDefault(q => q.PublicKey == question.Answers[i].PublicKey);
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
            var children = new List<IAnswer> { new Answer { AnswerText = "some text" }, new Answer(), new Answer() };
            completeQuestion.Answers = children;
            Assert.AreEqual(completeQuestion.AnswerDate, DateTime.Today);
            Assert.AreEqual(completeQuestion.AnswerOrder, Order.MaxMin);
            Assert.AreEqual(completeQuestion.ConditionExpression, "some expression");
            Assert.AreEqual(completeQuestion.Instructions, "some instructions");
            Assert.AreEqual(completeQuestion.QuestionText, "question text");
            Assert.AreEqual(completeQuestion.QuestionType, QuestionType.SingleOption);
            Assert.AreEqual(completeQuestion.StataExportCaption, "some stata export caption");
            Assert.AreEqual(completeQuestion.ValidationExpression, "some validation expression");
            Assert.AreEqual(completeQuestion.Answers.Count, 3);
            Assert.AreEqual(((Answer)completeQuestion.Answers[0]).AnswerText, "some text");
        }


        /// <summary>
        /// The cloned qeustion_ target is properly set.
        /// </summary>
        [Test]
        public void ClonedCompleteQeustion_TargetIsProperlySet()
        {
            // List<IGroup> groups = new List<IGroup>() { new Group("test") };
            var answers = new List<IAnswer> { new Answer() { AnswerText = "hi" }, new Answer() { AnswerText = "there" }, new Answer() };

            var question = new SingleCompleteQuestion()
            {
                ConditionExpression = "expr",
                Instructions = "instructions",
                AnswerOrder = Order.Random,
                StataExportCaption = "stata",
                Answers = answers,
                ConditionalDependentGroups = new List<Guid>() { Guid.NewGuid() },
                AddSingleAttr = "testAttr"
            };

            var target = question.Clone() as SingleCompleteQuestion;
            PropertyInfo[] propertiesForCheck = typeof(ICompleteQuestion).GetPublicPropertiesExcept("Parent", "Answers");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {
                Assert.AreEqual(publicProperty.GetValue(question, null), publicProperty.GetValue(target, null));
            }

            Assert.AreEqual(question.Answers.Count, target.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = target.Answers.FirstOrDefault(q => q.PublicKey == question.Answers[i].PublicKey);
                Assert.IsTrue(answer != null);

                Assert.IsTrue(!answer.Equals(question.Answers[i])); // they are interfaces and Equals uses Reference equality
            }
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