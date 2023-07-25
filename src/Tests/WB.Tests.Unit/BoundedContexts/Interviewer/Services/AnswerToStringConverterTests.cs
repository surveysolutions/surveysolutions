using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
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
        [TestCase(null, QuestionType.Numeric, null)]
        [TestCase(null, (QuestionType)777 /* any type */, null)]
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
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, questionId, questionnaire);

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
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(dt, Id.g1, questionnaire);

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
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(dt, Id.g1, questionnaire);

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
            var exception = Assert.Catch<NotSupportedException>(() => converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(dt, Id.g1, questionnaire));

            // assert
            Assert.That(exception, Is.Not.Null);
        }
        
        [Test]
        [SetCulture("pr-PR")]
        [TestCase(Double.NaN, "NaN", "NaN")]
        [TestCase(7777777.77777, "7777777.77777", "7,777,777.77777")]
        public void should_not_use_local_user_culture_of_decimal_question(double value, string result, string formatingResult)
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericRealQuestion(Id.g1, preFilled: true),
                    Create.Entity.NumericRealQuestion(Id.g2, useFomatting: true, preFilled: true))
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = NumericRealAnswer.FromDouble(value) as AbstractAnswer;

            // act
            var stringAnswer1 = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);
            var stringAnswer2 = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g2, questionnaire);

            // assert
            Assert.That(stringAnswer1, Is.EqualTo(result)); 
            Assert.That(stringAnswer2, Is.EqualTo(formatingResult));
        }
        
        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_text_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(Id.g1, preFilled: true))
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = TextAnswer.FromString("answer") as AbstractAnswer;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("answer"));
        }
        
        [Test]
        [SetCulture("pr-PR")]
        public void should_not_use_local_user_culture_of_integer_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(Id.g1, useFormatting: true, isPrefilled: true),
                    Create.Entity.NumericIntegerQuestion(Id.g2, useFormatting: false, isPrefilled: true)
                    )
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = NumericIntegerAnswer.FromInt(7777777) as AbstractAnswer;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);
            var stringAnswer2 = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g2, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("7,777,777"));
            Assert.That(stringAnswer2, Is.EqualTo("7777777"));
        }
        
        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_single_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleQuestion(Id.g1, isPrefilled: true, options: new List<Answer>()
                    {
                        new Answer() { AnswerCode = 777, AnswerText = "option 777"}
                    }))
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = CategoricalFixedSingleOptionAnswer.FromInt(777) as AbstractAnswer;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("option 777"));
        }
        
        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_gps_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.GpsCoordinateQuestion(Id.g1, isPrefilled: true))
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = GpsAnswer.FromGeoPosition(new GeoPosition(1, 2, 3, 4, DateTimeOffset.Now)) as AbstractAnswer;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("1, 2"));
        }

        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_datetime_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: true, preFilled: true),
                    Create.Entity.DateTimeQuestion(Id.g2, isTimestamp: false, preFilled: true)
                    )
            );
            var converter = Create.Service.AnswerToStringConverter();
            var answer = DateTimeAnswer.FromDateTime(new DateTime(2023, 7, 25, 13, 24, 33, DateTimeKind.Utc)) as AbstractAnswer;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);
            var stringAnswer2 = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g2, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.EqualTo("2023-07-25 13:24:33"));
            Assert.That(stringAnswer2, Is.EqualTo("2023-07-25"));
        }
        
        [Test]
        [SetCulture("fr-FR")]
        public void should_not_use_local_user_culture_of_null_answer_for_question()
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(Id.g1, isTimestamp: true, preFilled: true),
                    Create.Entity.DateTimeQuestion(Id.g2, isTimestamp: false, preFilled: true)
                    )
            );
            var converter = Create.Service.AnswerToStringConverter();
            AbstractAnswer answer = null;

            // act
            var stringAnswer = converter.GetUiStringAnswerForIdentifyingQuestionOrThrow(answer, Id.g1, questionnaire);

            // assert
            Assert.That(stringAnswer, Is.Null);
        }
    }
}
