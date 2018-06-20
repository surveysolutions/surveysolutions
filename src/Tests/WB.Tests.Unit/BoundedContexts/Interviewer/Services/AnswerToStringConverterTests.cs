using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
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

        [Test]
        public void should_not_change_timezone_of_date_question()
        {
            DateTime dt = new DateTime(2010, 4, 15);

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: false))
                );
            var converter = Create.Service.AnswerToStringConverter();

            // act
            var stringAnswer = converter.Convert(dt, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("2010-04-15"));
        }


        [Test]
        public void should_not_use_local_user_timezone_of_date_question_for_timestamp_question()
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime dt =TimeZoneInfo.ConvertTimeBySystemTimeZoneId(new DateTime(2010, 4, 15, 14, 30, 0), tzi.StandardName);
            
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: true))
            );
            var converter = Create.Service.AnswerToStringConverter();

            // act
            var stringAnswer = converter.Convert(dt, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo(dt.ToString(DateTimeFormat.DateWithTimeFormat)));
        }
    }
}
