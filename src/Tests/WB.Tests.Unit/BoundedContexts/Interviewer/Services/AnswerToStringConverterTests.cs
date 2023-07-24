using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
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
            var questionnaire = Mock.Of<IQuestionnaire>(
                   q => q.GetQuestionType(questionId) == questionType
                && q.GetAnswerOptionTitle(questionId, 1, null) == "title1"
                && q.GetAnswerOptionTitle(questionId, 2, null) == "title2"
                && q.IsIdentifying(questionId) == true);

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
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: false, preFilled: true))
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
            DateTime dt = new DateTimeOffset(new DateTime(2010, 4, 15, 14, 30, 0), TimeSpan.FromHours(-4)).DateTime;
            
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: true, preFilled: true))
            );
            var converter = Create.Service.AnswerToStringConverter();

            // act
            var stringAnswer = converter.Convert(dt, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo(dt.ToString(DateTimeFormat.DateWithTimeFormat)));
        }

        [Test]
        public void when_convert_answer_to_string_and_question_is_not_identifying_then_should_throw_not_supported_exception()
        {
            DateTime dt = new DateTime(2010, 4, 15, 14, 30, 0);

            var questionnaire = Create.Entity.PlainQuestionnaire(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: false, preFilled: false)
            );
            var converter = Create.Service.AnswerToStringConverter();

            // act
            var exception = Assert.Catch<NotSupportedException>(() => converter.Convert(dt, Id.g1, questionnaire));

            // assert
            Assert.That(exception, Is.Not.Null);
        }
        
        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_decimal_question_for_numeric_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericRealQuestion(Id.g1, preFilled: true))
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = NumericRealAnswer.FromDouble(Double.NaN) as AbstractAnswer;

            // act
            var stringAnswer = converter.Convert(answer, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("NaN"));
        }
    }
}
