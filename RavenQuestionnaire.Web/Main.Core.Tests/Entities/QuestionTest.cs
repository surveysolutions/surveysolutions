﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionTest.cs" company="">
//   
// </copyright>
// <summary>
//   The question test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Tests.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Main.Core.AbstractFactories;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Question;

    using NUnit.Framework;

    using RavenQuestionnaire.Core.Tests.Utils;

    /// <summary>
    /// The question test.
    /// </summary>
    [TestFixture]
    public class QuestionTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The explicit conversion_ valid question_ all field are converted.
        /// </summary>
        [Test]
        public void ExplicitConversion_ValidQuestion_AllFieldAreConverted()
        {
            // List<IGroup> groups = new List<IGroup>() { new Group("test") };
            var answers = new List<IAnswer> { new Answer(), new Answer(), new Answer() };

            var question = new SingleQuestion(Guid.NewGuid(), "test")
                {
                    ConditionExpression = "expr", 
                    Instructions = "instructions", 
                    AnswerOrder = Order.Random, 
                    StataExportCaption = "stata", 
                    Answers = answers
                };
            var target = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
            PropertyInfo[] propertiesForCheck = typeof(IQuestion).GetPublicPropertiesExcept(
                "PublicKey", "Children", "Parent", "Answers");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {
                Assert.AreEqual(publicProperty.GetValue(question, null), publicProperty.GetValue(target, null));
            }

            Assert.AreEqual(question.Answers.Count, target.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = target.Answers.FirstOrDefault(q => q.PublicKey == question.Answers[i].PublicKey);
                Assert.IsTrue(answer != null);
            }
        }

        /// <summary>
        /// The when set condition expression_ expression is added.
        /// </summary>
        [Test]
        public void WhenSetConditionExpression_ExpressionIsAdded()
        {
            var question = new SingleQuestion();
            question.ConditionExpression = "some expression";
            Assert.AreEqual(question.ConditionExpression, "some expression");
        }

        #endregion
    }
}