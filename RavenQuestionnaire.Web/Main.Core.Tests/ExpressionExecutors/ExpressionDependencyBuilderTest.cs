// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionDependencyBuilderTest.cs" company="">
//   
// </copyright>
// <summary>
//   The expression dependency builder test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Tests.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The expression dependency builder test.
    /// </summary>
    [TestFixture]
    public class ExpressionDependencyBuilderTest
    {

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
        }

        /// <summary>
        /// The handle tree test.
        /// </summary>
        [Test]
        public void HandleTreeTest()
        {
            QuestionnaireDocument doc = new QuestionnaireDocument();
            var mainGroup = new Group("Main Group");

            var question1 = new NumericQuestion()
                {
                    PublicKey = Guid.NewGuid()
                };

            var question2 = new NumericQuestion
                { 
                    ConditionExpression = string.Format("[{0}] > 3", question1.PublicKey),
                    PublicKey = Guid.NewGuid()
                };

            var subGroup1 = new Group("Sub Group1")
                { 
                    ConditionExpression = string.Format("[{0}] > 1", question1.PublicKey),
                    PublicKey = Guid.NewGuid()
                };

            var subGroup2 = new Group("Sub Group2")
            {
                ConditionExpression = string.Format("[{0}] > 1 and [{1}] = 1", question1.PublicKey, question2.PublicKey)
            };

            var question2_1 = new NumericQuestion
                { 
                    ConditionExpression = string.Format("[{0}] > 3", question1.PublicKey),
                    PublicKey = Guid.NewGuid()
                };

            subGroup2.Add(question2_1, null);

            mainGroup.Add(question1, null);
            mainGroup.Add(question2, null);

            mainGroup.Add(subGroup1, null);

            mainGroup.Add(subGroup2, null);

            doc.Add(mainGroup, null);

            var dependentQuestions = new Dictionary<Guid, List<Guid>>();
            var dependentGroups = new Dictionary<Guid, List<Guid>>();

            ExpressionDependencyBuilder.HandleTree(doc, dependentQuestions, dependentGroups);

            Assert.AreEqual(dependentQuestions.Count, 1);
            Assert.AreEqual(dependentGroups.Count, 2);

            Assert.AreEqual(dependentGroups.ContainsKey(question2.PublicKey), true);

            Assert.AreEqual(dependentGroups.ContainsKey(question1.PublicKey), true);
            Assert.AreEqual(dependentGroups[question1.PublicKey].Count, 2);

            Assert.AreEqual(dependentQuestions.ContainsKey(question1.PublicKey), true);
            Assert.AreEqual(dependentQuestions[question1.PublicKey].Count, 2);
        }

        /// <summary>
        /// The handle completed test.
        /// </summary>
        [Test]
        public void HandleCompletedTest()
        {
            var doc = new CompleteQuestionnaireDocument();
            var question = new MultyOptionsCompleteQuestion(string.Empty);
            
            var question2 = new SingleCompleteQuestion(string.Empty);
            question2.ConditionExpression = "contains([" + question.PublicKey + "],1)";

            var question3 = new MultyOptionsCompleteQuestion(string.Empty);
            question3.ConditionExpression = "[" + question2.PublicKey + "]=1";

            doc.Children.Add(question);
            doc.Children.Add(question2);
            doc.Children.Add(question3);

            Dictionary<Guid, List<Guid>> dependentQuestions = new Dictionary<Guid, List<Guid>>();
            Dictionary<Guid, List<Guid>> dependentGroups = new Dictionary<Guid, List<Guid>>();

            ExpressionDependencyBuilder.HandleTree(doc, dependentQuestions, dependentGroups);

            Assert.AreEqual(dependentQuestions.Count, 2);
            
            Assert.AreEqual(dependentQuestions.ContainsKey(question2.PublicKey), true);
            Assert.AreEqual(dependentQuestions[question2.PublicKey].Contains(question3.PublicKey), true);

            Assert.AreEqual(dependentQuestions.ContainsKey(question.PublicKey), true);
            Assert.AreEqual(dependentQuestions[question.PublicKey].Contains(question2.PublicKey), true);
            
            Assert.AreEqual(dependentGroups.Count, 0);
        }


        /// <summary>
        /// The collect changes test.
        /// </summary>
        [Test]
        public void EmptyConditions_Produce_Empty_Results()
        {
            var doc = new QuestionnaireDocument();

            var mainGroup = new Group("Main Group");

            var question1 = new NumericQuestion()
            {
                PublicKey = Guid.NewGuid()
            };

            var question2 = new NumericQuestion
            {
                ConditionExpression = "3 = 3",
                PublicKey = Guid.NewGuid()
            };

            var question3 = new NumericQuestion
            {
                ConditionExpression = string.Format("'[{0}]' = '[{0}]'", question1.PublicKey),
                PublicKey = Guid.NewGuid()
            };

            var group1 = new Group() { ConditionExpression = "3-2> 0 and 4" };

            mainGroup.Add(question1, null);
            mainGroup.Add(question2, null);
            group1.Add(question3, null);
            mainGroup.Add(group1, null);
            doc.Add(mainGroup, null);

            // var completedQ = (CompleteQuestionnaireDocument)doc;

            Dictionary<Guid, List<Guid>> dependentQuestions = new Dictionary<Guid, List<Guid>>();
            Dictionary<Guid, List<Guid>> dependentGroups = new Dictionary<Guid, List<Guid>>();


            ExpressionDependencyBuilder.HandleTree(doc, dependentQuestions, dependentGroups);

            Assert.AreEqual(dependentQuestions.Count, 0);
            Assert.AreEqual(dependentGroups.Count, 0);
        }
    }
}
