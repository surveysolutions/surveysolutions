using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(ImportDataVerifier))]
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_non_roster_file_should_return_PL0004_error()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var actualRosterName = "nonrosterfile";
            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: actualRosterName, fileName: actualRosterName);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFile(preloadedFile, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0004"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(actualRosterName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(actualRosterName));
        }

        [Test]
        public void when_verify_roster_file_should_return_empty_errors()
        {
            // arrange
            var expectedRosterName = "someRoster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: expectedRosterName)}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: expectedRosterName);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFile(preloadedFile, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_roster_file_in_lower_case_should_return_empty_errors()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "someroster");

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFile(preloadedFile, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_main_questionnaire_file_should_return_empty_errors()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "Questionnaire");

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFile(preloadedFile, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_main_questionnaire_file_in_lower_case_should_return_empty_errors()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter());
            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "questionnaire");
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFile(preloadedFile, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }
    }
}
