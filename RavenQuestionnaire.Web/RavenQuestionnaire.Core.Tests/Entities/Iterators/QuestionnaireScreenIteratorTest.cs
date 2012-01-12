using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [Test]
        public void WhenEmptyQuestionnaireIsPassed_ExceptionIsThrowed()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            Assert.Throws<ArgumentException>(
                () => new QuestionnaireScreenIterator(questionnaire));
        }
        [Test]
        public void First_FirstItemIsReturned()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Groups.Add(
                new 
                    CompleteGroup("first"));
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire);
            Assert.AreEqual(iterator.First.GroupText, "first");

            var takeNext = iterator.Next;
            Assert.AreEqual(iterator.First.GroupText, "first");
        }

        [Test]
        public void Iteration_WithoutConditions_GeneralTestForIteration()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("first"));
            questionnaire.GetInnerDocument().Groups.Add(
                new CompleteGroup("second"));
            var iterator = new QuestionnaireScreenIterator(questionnaire);

            /* Assert.AreEqual(iterator.Next.QuestionText, "first");*/
            Assert.AreEqual(iterator.IsDone, false);
            Assert.AreEqual(iterator.Next.GroupText, "second");
            Assert.AreEqual(iterator.IsDone, true);
            Assert.AreEqual(iterator.Previous.GroupText, "first");
            Assert.AreEqual(iterator.IsDone, false);
        }
    }
}
