// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireScreenIteratorTest.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire screen iterator test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Tests.Entities.Iterators
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Iterators;
    using Main.Core.Entities.SubEntities.Complete;

    using NUnit.Framework;

    /// <summary>
    /// The questionnaire screen iterator test.
    /// </summary>
    [TestFixture]
    public class QuestionnaireScreenIteratorTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
        }

        /// <summary>
        /// The first_ first item is returned.
        /// </summary>
        [Test]
        public void First_FirstItemIsReturned()
        {
            var questionnaire = new CompleteQuestionnaireDocument();
            questionnaire.Children.Add(new CompleteGroup("first"));
            questionnaire.Children.Add(new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire);
            Assert.AreEqual(iterator.First().Title, "first");

            var takeNext = iterator.Next;
            Assert.AreEqual(iterator.First().Title, "first");
        }

        /// <summary>
        /// The iteration_ without conditions_ general test for iteration.
        /// </summary>
        [Test]
        public void Iteration_WithoutConditions_GeneralTestForIteration()
        {
            var questionnaire = new CompleteQuestionnaireDocument();
            questionnaire.Children.Add(new CompleteGroup("first"));
            questionnaire.Children.Add(new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire);

            /* Assert.AreEqual(iterator.Next.Title, "first");*/
            Assert.AreEqual(iterator.MoveNext(), true);
            Assert.AreEqual(iterator.Current.Title, "second");
            Assert.AreEqual(iterator.MoveNext(), false);
            Assert.AreEqual(iterator.Previous.Title, "first");
            Assert.AreEqual(iterator.MoveNext(), true);
            var takePrevious = iterator.Previous;
            Assert.AreEqual(iterator.Next.Title, "second");
        }

        /// <summary>
        /// The when empty questionnaire is passed_ exception is throwed.
        /// </summary>
        [Test]
        public void WhenEmptyQuestionnaireIsPassed_ExceptionIsThrowed()
        {
            Assert.Throws<ArgumentException>(() => new QuestionnaireScreenIterator(new CompleteQuestionnaireDocument()));
        }

        #endregion
    }
}