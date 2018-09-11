using System;
using System.Linq;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_and_quantity_is_not_integer_should_return_PL0035_error()
        {
            // arrange
            string quantity = "not integer quantity";

            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(quantity));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0035"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(quantity));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_quantity_is_negative_integer_should_return_PL0036_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(parsedQuantity: -2));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0036"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo("-2"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_quantity_is_infinity_should_return_empty_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, quantity: Create.Entity.AssignmentQuantity(parsedQuantity: -1));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_answers_and_interview_id_is_empty_should_return_PL0042_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, interviewId: Create.Entity.AssignmentInterviewId(""));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0042"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(""));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_text_question_has_invalid_text_mask_should_return_PL0014_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string textVariableName = "tXt";
            var answer = "B3";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(variable: textVariableName, mask: "A#")));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new []{ Create.Entity.AssignmentTextAnswer(textVariableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(textVariableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_text_question_has_invalid_regexp_symbols_in_mask_should_not_throw_an_exception()
        {
            // arrange
            var fileName = "mainfile.tab";
            string textVariableName = "tXt";
            var answer = "239-991-3634";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(variable: textVariableName, mask: "+###-###-####")));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentTextAnswer(textVariableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            // assert
            Assert.DoesNotThrow(() => verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray());
        }

        [Test]
        public void when_verify_answers_and_categorical_single_question_has_unknown_option_code_should_return_PL0014_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "categorical";
            var answer = 11;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(variable: variableName, answerCodes: new[] {1m, 2m})));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentCategoricalSingleAnswer(variableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_categorical_single_question_with_comma_in_option_code_should_return_PL0014_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "categorical";
            string answer = "999,99";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(variable: variableName, answerCodes: new[] { 1m, 2m })));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentCategoricalSingleAnswer(variableName, value: answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_categorical_multi_question_has_unknown_option_code_should_return_2_PL0014_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "categorical";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(variable: variableName, answers: new[] { 1, 2, 3, 4 })));

            string unknown_answer_1 = "a";
            string unknown_answer_2 = "1,1";

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentMultiAnswer(variableName,
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__1", 1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__2", value: unknown_answer_1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__3", value: unknown_answer_2))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));
            Assert.That(errors[0].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknown_answer_1));
            Assert.That(errors[0].References.First().Column, Is.EqualTo($"{variableName}[2]"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[1].Code, Is.EqualTo("PL0014"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(unknown_answer_2));
            Assert.That(errors[1].References.First().Column, Is.EqualTo($"{variableName}[3]"));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_datetime_question_has_invalid_date_should_return_PL0016_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "dt";
            var invalid_date = "201/22/2009";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.DateTimeQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentDateTimeAnswer(variableName, value: invalid_date) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0016"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalid_date));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        public void when_verify_answers_and_integer_question_has_invalid_value_should_return_PL0018_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "intQ";
            var invalid_value = "B3";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentIntegerAnswer(variableName, value: invalid_value) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0018"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalid_value));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        public void when_verify_answers_and_double_question_has_invalid_value_should_return_PL0019_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "doubleQ";
            var invalid_value = "B3.3";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentDoubleAnswer(variableName, value: invalid_value) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0019"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalid_value));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_latitude_longitude_altitude_accuracy_have_invalid_values_should_return_4_PL0017_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            string unknown_latitude = "lat";
            string unknown_longitude = "long";
            string unknown_altitude = "alt";
            string unknown_accuracy = "acc";

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer($"{variableName}__latitude", value: unknown_latitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__longitude", value: unknown_longitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__altitude", value: unknown_altitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__accuracy", value: unknown_accuracy))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(4));
            Assert.That(errors[0].Code, Is.EqualTo("PL0017"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(unknown_latitude));
            Assert.That(errors[0].References.First().Column, Is.EqualTo($"{variableName}[latitude]"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[1].Code, Is.EqualTo("PL0017"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(unknown_longitude));
            Assert.That(errors[1].References.First().Column, Is.EqualTo($"{variableName}[longitude]"));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[2].Code, Is.EqualTo("PL0017"));
            Assert.That(errors[2].References.First().Content, Is.EqualTo(unknown_altitude));
            Assert.That(errors[2].References.First().Column, Is.EqualTo($"{variableName}[altitude]"));
            Assert.That(errors[2].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[3].Code, Is.EqualTo("PL0017"));
            Assert.That(errors[3].References.First().Content, Is.EqualTo(unknown_accuracy));
            Assert.That(errors[3].References.First().Column, Is.EqualTo($"{variableName}[accuracy]"));
            Assert.That(errors[3].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_integer_roster_size_question_has_value_less_than_1_return_PL0022_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "rsq";
            int answer = -1;

            var rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName, id: rosterSizeQuestionId),
                Create.Entity.NumericRoster(rosterSizeQuestionId: rosterSizeQuestionId)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentIntegerAnswer(variableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0022"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_integer_long_roster_size_question_has_value_more_than_max_answers_count_return_PL0029_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "rsq";
            int answer = 201;

            var rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName, id: rosterSizeQuestionId),
                Create.Entity.NumericRoster(rosterSizeQuestionId: rosterSizeQuestionId)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentIntegerAnswer(variableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0029"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_integer_roster_size_question_has_value_more_than_max_answers_count_return_PL0029_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "rsq";
            int answer = 61;

            var rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName, id: rosterSizeQuestionId),
                Create.Entity.NumericRoster(rosterSizeQuestionId: rosterSizeQuestionId,
                    children: Enumerable.Range(0, 31).Select(x => Create.Entity.TextQuestion()))));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentIntegerAnswer(variableName, answer) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0029"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_have_latitude_but_dont_have_longitude_should_return_PL0030_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer("longitude"),
                        Create.Entity.AssignmentDoubleAnswer("latitude", 60))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0030"));
            Assert.That(errors[0].References.First().Content, Is.Null);
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_have_latitude_but_dont_have_longitude_and_question_variable_in_caps_should_return_PL0030_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "GPS";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer("longitude"),
                        Create.Entity.AssignmentDoubleAnswer("latitude", 60))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0030"));
            Assert.That(errors[0].References.First().Content, Is.Null);
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_have_longitude_but_dont_have_latitude_should_return_PL0030_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer("longitude", 120),
                        Create.Entity.AssignmentDoubleAnswer("latitude"))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0030"));
            Assert.That(errors[0].References.First().Content, Is.Null);
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_latitude_more_than_90_should_return_PL0032_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";
            var answer = 91;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer(variableName, 120, variable: "longitude"),
                        Create.Entity.AssignmentDoubleAnswer(variableName, answer, variable: "latitude"))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0032"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_longitude_more_than_180_should_return_PL0033_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";
            var answer = 1811;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer(variableName, answer, variable: "longitude"),
                        Create.Entity.AssignmentDoubleAnswer(variableName, 60, variable: "latitude"))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0033"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_latitude_less_than_minus_90_should_return_PL0032_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";
            var answer = -91;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer(variableName, 120, variable: "longitude"),
                        Create.Entity.AssignmentDoubleAnswer(variableName, answer, variable: "latitude"))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0032"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_longitude_less_than_minus_180_should_return_PL0033_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";
            var answer = -181;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer(variableName, answer, variable: "longitude"),
                        Create.Entity.AssignmentDoubleAnswer(variableName, 60, variable: "latitude"))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0033"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(answer.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_gps_latitude_longitude_altitude_accuracy_have_comma_in_value_should_return_4_PL0034_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "gps";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(variable: variableName)));

            string invalid_latitude = "4,5";
            string invalid_longitude = "5,6";
            string invalid_altitude = "7,8";
            string invalid_accuracy = "8,9";

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentGpsAnswer(variableName, Create.Entity.AssignmentDoubleAnswer($"{variableName}__latitude", value: invalid_latitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__longitude", value: invalid_longitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__altitude", value: invalid_altitude),
                        Create.Entity.AssignmentDoubleAnswer($"{variableName}__accuracy", value: invalid_accuracy))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(4));
            Assert.That(errors[0].Code, Is.EqualTo("PL0034"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalid_latitude));
            Assert.That(errors[0].References.First().Column, Is.EqualTo($"{variableName}[latitude]"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[1].Code, Is.EqualTo("PL0034"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo(invalid_longitude));
            Assert.That(errors[1].References.First().Column, Is.EqualTo($"{variableName}[longitude]"));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[2].Code, Is.EqualTo("PL0034"));
            Assert.That(errors[2].References.First().Content, Is.EqualTo(invalid_altitude));
            Assert.That(errors[2].References.First().Column, Is.EqualTo($"{variableName}[altitude]"));
            Assert.That(errors[2].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[3].Code, Is.EqualTo("PL0034"));
            Assert.That(errors[3].References.First().Content, Is.EqualTo(invalid_accuracy));
            Assert.That(errors[3].References.First().Column, Is.EqualTo($"{variableName}[accuracy]"));
            Assert.That(errors[3].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_double_question_has_comma_in_value_should_return_PL0034_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "doubleQ";
            var invalid_value = "3,3";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, answers: new[] { Create.Entity.AssignmentDoubleAnswer(variableName, value: invalid_value) });
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0034"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(invalid_value));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_categorical_multi_question_has_more_than_max_answers_count_should_return_PL0041_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "categorical";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(variable: variableName, answers: new []{ 1, 2 }, maxAllowedAnswers: 1)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentMultiAnswer(variableName, Create.Entity.AssignmentIntegerAnswer($"{variableName}__1", 1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__2", 2))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0041"));
            Assert.That(errors[0].References.First().Content, Is.Null);
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_categorical_multi_question_has_more_than_max_answers_count_and_question_variable_in_caps_should_return_PL0041_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "CATEGORICAL";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(variable: variableName, answers: new[] { 1, 2 }, maxAllowedAnswers: 1)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentMultiAnswer(variableName, Create.Entity.AssignmentIntegerAnswer($"{variableName}__1", 1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__2", 2))
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0041"));
            Assert.That(errors[0].References.First().Content, Is.Null);
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_not_parsed_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric__id";
            var code = "not parsed code";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter());

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(variableName, value: code)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_is_empty_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric__id";
            var code = "";

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter());

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(variableName, value: code)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(variableName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_by_fixed_roster_doesnot_exist_in_fixed_roster_codes_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";
            var columnName = $"{variableName}__id";
            var code = 33;

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(variable: variableName, fixedTitles: Create.Entity.FixedTitles(11, 22))));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(columnName, code, variable: variableName)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(columnName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_by_numeric_roster_and_value_less_than_0_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";
            var columnName = $"{variableName}__id";
            var code = -2;

            var rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: rosterSizeId),
                Create.Entity.NumericRoster(variable: variableName, rosterSizeQuestionId: rosterSizeId)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(columnName, code, variable: variableName)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(columnName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_by_textlist_roster_and_value_less_than_0_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";
            var columnName = $"{variableName}__id";
            var code = -2;

            var rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(rosterSizeId),
                Create.Entity.ListRoster(variable: variableName, rosterSizeQuestionId: rosterSizeId)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(columnName, code, variable: variableName)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(columnName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_roster_instance_code_by_categorical_roster_and_value_less_than_0_should_return_PL0009_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";
            var columnName = $"{variableName}__id";
            var code = 4;

            var rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(rosterSizeId, answers: new []{1,2,3}),
                Create.Entity.ListRoster(variable: variableName, rosterSizeQuestionId: rosterSizeId)));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentRosterInstanceCode(columnName, code, variable: variableName)
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0009"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(code.ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(columnName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_on_mustiselect_question_less_than_1_should_return_PL0050_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";

            var questionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, variable: variableName, answers: new []{1,2,3})));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentMultiAnswer(variableName, 
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__1", 1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__2", -1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__3", 20),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__4", null),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__5", -20),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__6", 0)
                        )
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));

            Assert.That(errors[0].Code, Is.EqualTo("PL0050"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo((-1).ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo($"{variableName}[2]"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[1].Code, Is.EqualTo("PL0050"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo((-20).ToString()));
            Assert.That(errors[1].References.First().Column, Is.EqualTo($"{variableName}[5]"));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_on_yesno_question_less_than_0_should_return_PL0050_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            string variableName = "ric";
            var columnName = $"{variableName}__id";

            var questionId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, isYesNo: true, variable: variableName, answers: new []{1,2,3})));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                answers: new[]
                {
                    Create.Entity.AssignmentMultiAnswer(variableName, 
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__1", 1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__2", -1),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__3", 20),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__4", -20),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__5", null),
                        Create.Entity.AssignmentIntegerAnswer($"{variableName}__6", 0)
                        )
                });

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyAnswers(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(2));

            Assert.That(errors[0].Code, Is.EqualTo("PL0050"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo((-1).ToString()));
            Assert.That(errors[0].References.First().Column, Is.EqualTo($"{variableName}[2]"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));

            Assert.That(errors[1].Code, Is.EqualTo("PL0050"));
            Assert.That(errors[1].References.First().Content, Is.EqualTo((-20).ToString()));
            Assert.That(errors[1].References.First().Column, Is.EqualTo($"{variableName}[4]"));
            Assert.That(errors[1].References.First().DataFile, Is.EqualTo(fileName));
        }
    }
}
