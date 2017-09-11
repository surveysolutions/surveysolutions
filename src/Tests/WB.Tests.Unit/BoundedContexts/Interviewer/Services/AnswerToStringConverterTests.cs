using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services
{
    [TestFixture]
    [TestOf(typeof(AnswerToStringConverter))]
    public class AnswerToStringConverterTests
    {
        [Test]
        [TestCase("string", QuestionType.Text, "string")]
        [TestCase(256, QuestionType.Numeric, "256")]
        [TestCase(123256, QuestionType.Numeric, "123256")]
        [TestCase("123256", QuestionType.Numeric, "123256")]
        [TestCase("2", QuestionType.SingleOption, "title2")]
        [TestCase("1,2[3]4", QuestionType.GpsCoordinates, "1, 2")]
        public void when_get_answer_it_should_stored_to_correct_string(object answer, QuestionType questionType, string result)
        {
            Guid questionId = Guid.NewGuid();
            var questionnaire = Mock.Of<IQuestionnaire>(q => q.GetQuestionType(questionId) == questionType
                && q.GetAnswerOptionTitle(questionId, 1) == "title1"
                && q.GetAnswerOptionTitle(questionId, 2) == "title2");

            var converter = Create.Service.AnswerToStringConverter();

            // act
            var stringAnswer = converter.Convert(answer, questionId, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo(result));
        }
    }
}