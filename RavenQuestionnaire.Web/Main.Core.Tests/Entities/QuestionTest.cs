using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Tests.Utils;

namespace Main.Core.Tests.Entities
{
    [TestFixture]
    public class QuestionTest
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void WhenSetConditionExpression_ExpressionIsAdded()
        {
            var question = new SingleQuestion();
            question.ConditionExpression = "some expression";
            Assert.AreEqual(question.ConditionExpression, "some expression");
        }

        [Test]
        public void ClonedQeustion_TargetIsProperlySet()
        {
            var answers = new List<Answer> { new Answer() { AnswerText = "hi" }, new Answer(){AnswerText = "there"}, new Answer() };

            var question = new SingleQuestion(Guid.NewGuid(), "test")
                {
                    ConditionExpression = "expr", 
                    Instructions = "instructions", 
                    AnswerOrder = Order.Random, 
                    StataExportCaption = "stata", 
                    Answers = answers,
                    ConditionalDependentGroups = new List<Guid>() { Guid.NewGuid() },
                    ConditionalDependentQuestions = new List<Guid>() { Guid.NewGuid() },
                    //QuestionsWhichCustomValidationDependsOnQuestion = new List<Guid>() { Guid.NewGuid() },
                    QuestionIdsInvolvedInCustomEnablementConditionOfQuestion = new List<Guid>() { Guid.NewGuid() },
                    QuestionIdsInvolvedInCustomValidationOfQuestion = new List<Guid>() { Guid.NewGuid() }
                };

            var target = question.Clone() as SingleQuestion;
            PropertyInfo[] propertiesForCheck = typeof(IQuestion).GetPublicPropertiesExcept("Parent", "Answers");
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
    }
}