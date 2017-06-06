using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(IdentifyingAnswerConverter))]
    [TestFixture]
    public class IdentifyingAnswerConverterTests
    {
        [Test]
        [TestCase(QuestionType.Text,           "text",       typeof(TextAnswer))]
        [TestCase(QuestionType.Numeric,        "1",          typeof(NumericIntegerAnswer))]
        [TestCase(QuestionType.SingleOption,   "1",          typeof(CategoricalFixedSingleOptionAnswer))]
        [TestCase(QuestionType.DateTime,       "11/01/2017", typeof(DateTimeAnswer))]
        [TestCase(QuestionType.GpsCoordinates, "55,47[2]7",  typeof(GpsAnswer))]
        public void When_GetAbstractAnswer_with_question_type_and_correct_answer_Then_should_return_correct_answer_type(QuestionType questionType, string stringAnswer, Type answerType)
        {
            //arrange
            var identifyingAnswerConverter = Create.Service.IdentifyingAnswerConverter();

            //act
            var questionAnswer = identifyingAnswerConverter.GetAbstractAnswer(questionType, stringAnswer);

            //assert
            Assert.That(questionAnswer.GetType(), Is.EqualTo(answerType));
        }
    }
}