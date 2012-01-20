using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Tests.Entities.Iterators
{
    [TestFixture]
    public class QuestionnaireScreenIteratorTest
    {
        public Mock<IIteratorContainer> iteratorContainerMock;
        [SetUp]
        public void CreateObjects()
        {
            iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void WhenEmptyQuestionnaireIsPassed_ExceptionIsThrowed()
        {
            Assert.Throws<ArgumentException>(
                () => new QuestionnaireScreenIterator(new CompleteQuestionnaireDocument()));
        }
        [Test]
        public void First_FirstItemIsReturned()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument(), iteratorContainerMock.Object);
            questionnaire.GetInnerDocument().Groups.Add(
                new 
                    CompleteGroup("first"));
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire.GetInnerDocument());
            Assert.AreEqual(iterator.First().GroupText, "first");

            var takeNext = iterator.Next;
            Assert.AreEqual(iterator.First().GroupText, "first");
        }

        [Test]
        public void Iteration_WithoutConditions_GeneralTestForIteration()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument(), iteratorContainerMock.Object);
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("first"));
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire.GetInnerDocument());

            /* Assert.AreEqual(iterator.Next.QuestionText, "first");*/
            Assert.AreEqual(iterator.MoveNext(), true);
            Assert.AreEqual(iterator.Current.GroupText, "second");
            Assert.AreEqual(iterator.MoveNext(), false);
            Assert.AreEqual(iterator.Previous.GroupText, "first");
            Assert.AreEqual(iterator.MoveNext(), true);
            var takePrevious =  iterator.Previous;
            Assert.AreEqual(iterator.Next.GroupText, "second");
        }
    }
}
