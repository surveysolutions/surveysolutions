using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_and_empty_responsible_should_return_empty_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, Create.Entity.AssignmentResponsible(""));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_answers_and_responsible_not_found_should_return_PL0026_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var responsibleName = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, Create.Entity.AssignmentResponsible(responsibleName));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0026"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(responsibleName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_web_assignment_assigned_to_supervisor_Should_return_0062_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var responsibleName = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneQuestion());

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                Create.Entity.AssignmentResponsible(responsibleName, new UserToVerify {SupervisorId = Id.gA}),
                assignmentWebMode: Create.Entity.AssignmentWebMode(true),
                quantity: Create.Entity.AssignmentQuantity(parsedQuantity: 5)
            );
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0062"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(responsibleName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }
        
        [Test]
        public void when_verify_answers_and_responsible_is_locked_should_return_PL0027_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var responsibleName = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, Create.Entity.AssignmentResponsible(responsibleName, Create.Entity.UserToVerify(true, Guid.NewGuid())));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0027"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(responsibleName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_responsible_not_headquarters_not_supervisor_and_not_interviewer_should_return_PL0028_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var responsibleName = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.TextQuestion()}));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, Create.Entity.AssignmentResponsible(responsibleName, Create.Entity.UserToVerify()));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0028"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(responsibleName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_answers_and_responsible_is_headquarters_should_return_empty_errors()
        {
            // arrange
            var fileName = "mainfile.tab";
            var responsibleName = "john doe";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName, Create.Entity.AssignmentResponsible(responsibleName, Create.Entity.UserToVerify(hqId: Guid.NewGuid())));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }
    }
}
