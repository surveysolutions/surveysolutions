using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
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
    }
}
