using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsImportFileConverter))]
    internal class AssignmentsImportFileConverterTests
    {
        [Test]
        public void when_getting_assignment_row_and_file_has_interview_id_value_should_return_row_with_interview_id_preloading_value()
        {
            //arrange
            var interviewId = "interview1";
            var column = "interview__Id";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                    Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(column, interviewId)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].InterviewIdValue, Is.Not.Null);
            Assert.That(assignmentRows[0].InterviewIdValue, Is.TypeOf<AssignmentInterviewId>());
            Assert.That(assignmentRows[0].InterviewIdValue.Value, Is.EqualTo(interviewId));
            Assert.That(assignmentRows[0].InterviewIdValue.Column, Is.EqualTo(column));
        }

        [Test]
        public void when_getting_assignment_row_and_file_has_quantity_value_should_return_row_with_quantity_preloading_value()
        {
            //arrange
            var quantity = "1";
            var column = "_Quantity";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(column, quantity)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Quantity, Is.Not.Null);
            Assert.That(assignmentRows[0].Quantity.Value, Is.EqualTo(quantity));
            Assert.That(assignmentRows[0].Quantity.Quantity, Is.EqualTo(1));
            Assert.That(assignmentRows[0].Quantity.Column, Is.EqualTo(column));
        }

        [Test]
        public void when_getting_assignment_row_and_file_has_responsible_value_should_return_row_with_responsible()
        {
            //arrange
            var responsible = "john";
            var column = "_ResponsiblE";

            var supervisorId = Guid.Parse("11111111111111111111111111111111");
            var interviewerId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(column, responsible)));

            var converter = Create.Service.AssignmentsImportFileConverter(
                userViewFactory: Create.Storage.UserViewFactory(Create.Entity.HqUser(interviewerId, supervisorId,
                    isLockedByHQ: true, userName: responsible)));

            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Responsible, Is.Not.Null);
            Assert.That(assignmentRows[0].Responsible.Value, Is.EqualTo(responsible));
            Assert.That(assignmentRows[0].Responsible.Responsible.InterviewerId, Is.EqualTo(interviewerId));
            Assert.That(assignmentRows[0].Responsible.Responsible.SupervisorId, Is.EqualTo(supervisorId));
            Assert.That(assignmentRows[0].Responsible.Responsible.IsSupervisorOrInterviewer, Is.EqualTo(true));
            Assert.That(assignmentRows[0].Responsible.Responsible.IsLocked, Is.EqualTo(true));
            Assert.That(assignmentRows[0].Responsible.Column, Is.EqualTo(column));
        }

        [Test]
        public void when_getting_assignment_row_and_nested_roster_file_has_roster_instance_values_should_return_row_with_roster_instance_codes()
        {
            //arrange
            var rosterName = "mainroster";
            var nestedRosterName = "nestedRoster";

            var rosterColumnName = $"{rosterName}__Id";
            var nestedRosterColumnName = "parentID2";

            var rosterInstanceCode = "1";
            var nestedRosterInstanceCode = "2";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(variable: rosterName, children: new[]
                    {
                        Create.Entity.FixedRoster(variable: nestedRosterName, children: new[] {Create.Entity.TextQuestion()})
                    })));

            
            var file = Create.Entity.PreloadedFile(nestedRosterName,
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(rosterColumnName, rosterInstanceCode),
                    Create.Entity.PreloadingValue(nestedRosterColumnName, nestedRosterInstanceCode)));

            var converter = Create.Service.AssignmentsImportFileConverter();

            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();

            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].RosterInstanceCodes.Length, Is.EqualTo(2));

            Assert.That(assignmentRows[0].RosterInstanceCodes[0].VariableName, Is.EqualTo(rosterColumnName.ToLower()));
            Assert.That(assignmentRows[0].RosterInstanceCodes[0].Code, Is.EqualTo(1));
            Assert.That(assignmentRows[0].RosterInstanceCodes[0].Value, Is.EqualTo(rosterInstanceCode));
            Assert.That(assignmentRows[0].RosterInstanceCodes[0].Column, Is.EqualTo(rosterColumnName));

            Assert.That(assignmentRows[0].RosterInstanceCodes[1].VariableName, Is.EqualTo(nestedRosterColumnName.ToLower()));
            Assert.That(assignmentRows[0].RosterInstanceCodes[1].Code, Is.EqualTo(2));
            Assert.That(assignmentRows[0].RosterInstanceCodes[1].Value, Is.EqualTo(nestedRosterInstanceCode));
            Assert.That(assignmentRows[0].RosterInstanceCodes[1].Column, Is.EqualTo(nestedRosterColumnName));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_integer_question_should_return_row_with_integer_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "123";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentIntegerAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentIntegerAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Answer, Is.EqualTo(123));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_text_question_should_return_row_with_text_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "some text";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentTextAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentTextAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_double_question_should_return_row_with_double_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "12.2";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericRealQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentDoubleAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentDoubleAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.Answer, Is.EqualTo(12.2));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_datetime_question_should_return_row_with_datetime_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "12/1/98 13:40:56";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.DateTimeQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentDateTimeAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentDateTimeAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.Answer, Is.EqualTo(new DateTime(1998, 12, 1, 13, 40, 56, DateTimeKind.Utc)));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_categorical_single_question_should_return_row_with_categorical_single_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "123";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(variable: variable, answerCodes: new[] {123m})));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentCategoricalSingleAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentCategoricalSingleAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.OptionCode, Is.EqualTo(123));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_qrbarcode_question_should_return_row_with_text_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "some barcode text";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.QRBarcodeQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentTextAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentTextAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_roster_instance_textlist_question_should_return_row_with_text_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var value = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentTextAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentTextAnswer;

            Assert.That(answer.Value, Is.EqualTo(value));
            Assert.That(answer.Column, Is.EqualTo(variable));
            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_textlist_question_should_return_row_with_multi_answers_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var listItem1SortIndex = "1";
            var listItem2SortIndex = "2";
            var listItem1ColumnName = $"{variable}__{listItem1SortIndex}";
            var listItem2ColumnName = $"{variable}__{listItem2SortIndex}";

            var listItem1 = "john";
            var listItem2 = "jack";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(listItem1SortIndex, listItem1, listItem1ColumnName),
                    Create.Entity.PreloadingValue(listItem2SortIndex, listItem2, listItem2ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentMultiAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentMultiAnswer;

            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
            Assert.That(answer.Values.Length, Is.EqualTo(2));

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentTextAnswer>());
            Assert.That(answer.Values[0].Value, Is.EqualTo(listItem1));
            Assert.That(answer.Values[0].Column, Is.EqualTo(listItem1ColumnName));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo(listItem1SortIndex));

            Assert.That(answer.Values[1], Is.TypeOf<AssignmentTextAnswer>());
            Assert.That(answer.Values[1].Value, Is.EqualTo(listItem2));
            Assert.That(answer.Values[1].Column, Is.EqualTo(listItem2ColumnName));
            Assert.That(answer.Values[1].VariableName, Is.EqualTo(listItem2SortIndex));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_gps_question_should_return_row_with_gps_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var gpsPropertyName1 = "Latitude";
            var gpsPropertyName2 = "tImestamp";

            var latitudeColumnName = $"{variable}__{gpsPropertyName1}";
            var timestampColumnName = $"{variable}__{gpsPropertyName2}";

            var latitudeAnswer = "123";
            var timestampAnswer = "12/1/98";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.GpsCoordinateQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(gpsPropertyName1, latitudeAnswer, latitudeColumnName),
                    Create.Entity.PreloadingValue(gpsPropertyName2, timestampAnswer, timestampColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentGpsAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentGpsAnswer;

            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
            Assert.That(answer.Values.Length, Is.EqualTo(2));

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentDoubleAnswer>());
            Assert.That(answer.Values[0].Value, Is.EqualTo(latitudeAnswer));
            Assert.That(answer.Values[0].Column, Is.EqualTo(latitudeColumnName));
            Assert.That(((AssignmentDoubleAnswer)answer.Values[0]).Answer, Is.EqualTo(123));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo(gpsPropertyName1.ToLower()));

            Assert.That(answer.Values[1], Is.TypeOf<AssignmentDateTimeAnswer>());
            Assert.That(answer.Values[1].Value, Is.EqualTo(timestampAnswer));
            Assert.That(answer.Values[1].Column, Is.EqualTo(timestampColumnName));
            Assert.That(((AssignmentDateTimeAnswer)answer.Values[1]).Answer, Is.EqualTo(new DateTime(1998, 12, 1)));
            Assert.That(answer.Values[1].VariableName, Is.EqualTo(gpsPropertyName2.ToLower()));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_categorical_multi_question_should_return_row_with_multi_answers_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var optionCode1 = "1";
            var optionCode2 = "2";
            var optionCode1ColumnName = $"{variable}__{optionCode1}";
            var optionCode2ColumnName = $"{variable}__{optionCode2}";

            var optionCode1SortIndex = "22";
            var optionCode2SortIndex = "33";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(optionCode1, optionCode1SortIndex, optionCode1ColumnName),
                    Create.Entity.PreloadingValue(optionCode2, optionCode2SortIndex, optionCode2ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentMultiAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentMultiAnswer;

            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
            Assert.That(answer.Values.Length, Is.EqualTo(2));

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[0].Value, Is.EqualTo(optionCode1SortIndex));
            Assert.That(answer.Values[0].Column, Is.EqualTo(optionCode1ColumnName));
            Assert.That(((AssignmentIntegerAnswer)answer.Values[0]).Answer, Is.EqualTo(22));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo(optionCode1));

            Assert.That(answer.Values[1], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[1].Value, Is.EqualTo(optionCode2SortIndex));
            Assert.That(answer.Values[1].Column, Is.EqualTo(optionCode2ColumnName));
            Assert.That(((AssignmentIntegerAnswer)answer.Values[1]).Answer, Is.EqualTo(33));
            Assert.That(answer.Values[1].VariableName, Is.EqualTo(optionCode2));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_yes_no_question_should_return_row_with_multi_answers_preloading_value()
        {
            //arrange
            var variable = "questionId";
            var optionCode1 = "1";
            var optionCode2 = "2";
            var optionCode1ColumnName = $"{variable}__{optionCode1}";
            var optionCode2ColumnName = $"{variable}__{optionCode2}";

            var optionCode1SortIndex = "22";
            var optionCode2SortIndex = "33";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(optionCode1, optionCode1SortIndex, optionCode1ColumnName),
                    Create.Entity.PreloadingValue(optionCode2, optionCode2SortIndex, optionCode2ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(assignmentRows[0].Answers[0], Is.TypeOf<AssignmentMultiAnswer>());

            var answer = assignmentRows[0].Answers[0] as AssignmentMultiAnswer;

            Assert.That(answer.VariableName, Is.EqualTo(variable.ToLower()));
            Assert.That(answer.Values.Length, Is.EqualTo(2));

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[0].Value, Is.EqualTo(optionCode1SortIndex));
            Assert.That(answer.Values[0].Column, Is.EqualTo(optionCode1ColumnName));
            Assert.That(((AssignmentIntegerAnswer)answer.Values[0]).Answer, Is.EqualTo(22));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo(optionCode1));

            Assert.That(answer.Values[1], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[1].Value, Is.EqualTo(optionCode2SortIndex));
            Assert.That(answer.Values[1].Column, Is.EqualTo(optionCode2ColumnName));
            Assert.That(((AssignmentIntegerAnswer)answer.Values[1]).Answer, Is.EqualTo(33));
            Assert.That(answer.Values[1].VariableName, Is.EqualTo(optionCode2));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_categorical_multi_question_with_negative_option_code_should_return_row_with_multi_answers_preloading_value_with_n_to_minus_replacement()
        {
            //arrange
            var variable = "questionId";
            var optionCode1 = "n1";
            var optionCode1ColumnName = $"{variable}__{optionCode1}";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(optionCode1, "1", optionCode1ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            var answer = assignmentRows[0].Answers[0] as AssignmentMultiAnswer;

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[0].Column, Is.EqualTo(optionCode1ColumnName));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo("-1"));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_categorical_yes_no_question_with_negative_option_code_should_return_row_with_multi_answers_preloading_value_with_n_to_minus_replacement()
        {
            //arrange
            var variable = "questionId";
            var optionCode1 = "n1";
            var optionCode1ColumnName = $"{variable}__{optionCode1}";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(optionCode1, "1", optionCode1ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            var answer = assignmentRows[0].Answers[0] as AssignmentMultiAnswer;

            Assert.That(answer.Values[0], Is.TypeOf<AssignmentIntegerAnswer>());
            Assert.That(answer.Values[0].Column, Is.EqualTo(optionCode1ColumnName));
            Assert.That(answer.Values[0].VariableName, Is.EqualTo("-1"));
        }

        [Test]
        public void when_getting_assignment_row_with_answer_by_variable_should_return_row_without_answers()
        {
            //arrange
            var variable = "variableId";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.Variable(variableName: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, "1")));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Is.Empty);
        }

        [Test]
        public void when_getting_assignment_row_with_empty_answer_by_text_question_should_return_row_with_answer_with_empty_value()
        {
            //arrange
            var variable = "questionId";
            var value = "";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, value)));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(((AssignmentTextAnswer)assignmentRows[0].Answers[0]).Value, Is.Empty);
        }

        [Test]
        public void when_getting_assignment_row_with_2_empty_answers_by_categorical_multi_question_should_return_row_with_categorical_multi_answer_with_2_empty_values()
        {
            //arrange
            var variable = "questionId";
            var optionCode1 = "1";
            var optionCode2 = "2";
            var optionCode1ColumnName = $"{variable}__{optionCode1}";
            var optionCode2ColumnName = $"{variable}__{optionCode2}";

            var optionCode1SortIndex = "";
            var optionCode2SortIndex = "";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(variable: variable)));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue(optionCode1, optionCode1SortIndex, optionCode1ColumnName),
                    Create.Entity.PreloadingValue(optionCode2, optionCode2SortIndex, optionCode2ColumnName))));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Answers, Has.One.Items);
            Assert.That(((AssignmentMultiAnswer)assignmentRows[0].Answers[0]).Values, Has.Exactly(2).Items);
            Assert.That(((AssignmentMultiAnswer)assignmentRows[0].Answers[0]).Values[0].Value, Is.Empty);
            Assert.That(((AssignmentMultiAnswer)assignmentRows[0].Answers[0]).Values[1].Value, Is.Empty);
        }

        [Test]
        public void when_getting_assignment_row_with_1_row_should_return_row_with_row_index_equals_to_1()
        {
            //arrange
            var interviewId = "interview1";
            var column = "interview__Id";
            var expectedRowIndex = 2;

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var file = Create.Entity.PreloadedFile(rows: Create.Entity.PreloadingRow(expectedRowIndex,
                cells: new PreloadingCell[] {Create.Entity.PreloadingValue(column, interviewId)}));

            var converter = Create.Service.AssignmentsImportFileConverter();
            //act
            var assignmentRows = converter.GetAssignmentRows(file, questionnaire).ToArray();
            //assert
            Assert.That(assignmentRows, Has.One.Items);
            Assert.That(assignmentRows[0].Row, Is.EqualTo(expectedRowIndex));
        }
    }
}
