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
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.Entities.Iterators
{
    [TestFixture]
    public class QuestionnaireSimpleIteratorTest
    {
        [SetUp]
        public void CreateObjects()
        {

        }
        [Test]
        public void WhenEmptyQuestionnaireIsPassed_ExceptionIsThrowed()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Questionnaire= new QuestionnaireDocument();    
            Assert.Throws<ArgumentException>(
                () => new QuestionnaireSimpleIterator(questionnaire, new CompleteQuestionnaireConditionExecutor()));
        }
        [Test]
        public void First_FirstItemIsReturned()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Questionnaire = new QuestionnaireDocument();
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(
                new Question("first", QuestionType.DynamicInputList));
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(
                new Question("second", QuestionType.DynamicInputList));
            var iterator = new QuestionnaireSimpleIterator(questionnaire, new CompleteQuestionnaireConditionExecutor());
            Assert.AreEqual(iterator.First.QuestionText, "first");

            var takeNext = iterator.Next;
            Assert.AreEqual(iterator.First.QuestionText, "first");
        }

        [Test]
        public void Iteration_WithoutConditions_GeneralTestForIteration()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Questionnaire = new QuestionnaireDocument();
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(
                new Question("first", QuestionType.DynamicInputList));
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(
                new Question("second", QuestionType.DynamicInputList));
            var iterator = new QuestionnaireSimpleIterator(questionnaire, new CompleteQuestionnaireConditionExecutor());

           /* Assert.AreEqual(iterator.Next.QuestionText, "first");*/
            Assert.AreEqual(iterator.IsDone, false);
            Assert.AreEqual(iterator.Next.QuestionText, "second");
            Assert.AreEqual(iterator.IsDone, true);
            Assert.AreEqual(iterator.Previous.QuestionText, "first");
            Assert.AreEqual(iterator.IsDone, false);
        }

        [Test]
        public void Iteration_WithConditions_GeneralTestForIteration()
        {
            var questionnaire = new CompleteQuestionnaire(new CompleteQuestionnaireDocument());
            questionnaire.GetInnerDocument().Questionnaire = new QuestionnaireDocument();


            Question falseConditionQuestion = new Question("false", QuestionType.DynamicInputList);
            falseConditionQuestion.SetConditionExpression("5<1");
            Question trueConditionQuestion1 = new Question("true1", QuestionType.DynamicInputList);
            trueConditionQuestion1.SetConditionExpression("5>1");
            Question trueConditionQuestion2 = new Question("true2", QuestionType.DynamicInputList);
            trueConditionQuestion2.SetConditionExpression("5>1");
         
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(trueConditionQuestion1);
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(trueConditionQuestion2);
            questionnaire.GetInnerDocument().Questionnaire.Questions.Add(falseConditionQuestion);

            var iterator = new QuestionnaireSimpleIterator(questionnaire, new CompleteQuestionnaireConditionExecutor());

            Assert.AreEqual(iterator.Next, trueConditionQuestion2);
            Assert.AreEqual(iterator.IsDone, false);
            Assert.AreEqual(iterator.Next, null);
            Assert.AreEqual(iterator.IsDone, true);
            Assert.AreEqual(iterator.Previous, trueConditionQuestion2);
            Assert.AreEqual(iterator.IsDone, false);
        }
    }
}
