using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
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
            var arhiveFileName = "arhive.zip";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var actualRosterName = "nonrosterfile";
            var mainFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "questionnaire");
            var rosterFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: actualRosterName, fileName: actualRosterName);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] {mainFile, rosterFile}, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0004"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(actualRosterName));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(arhiveFileName));
        }

        [Test]
        public void when_verify_roster_file_should_return_empty_errors()
        {
            // arrange
            var arhiveFileName = "arhive.zip";
            var expectedRosterName = "someRoster";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: expectedRosterName)}));

            var mainFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "questionnaire");
            var rosterFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: expectedRosterName);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_roster_file_in_lower_case_should_return_empty_errors()
        {
            // arrange
            var arhiveFileName = "arhive.zip";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var mainFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "questionnaire");
            var rosterFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "someroster");

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_main_questionnaire_file_should_return_empty_errors()
        {
            // arrange
            var arhiveFileName = "arhive.zip";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "Questionnaire");

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_main_questionnaire_file_in_lower_case_should_return_empty_errors()
        {
            // arrange
            var arhiveFileName = "arhive.zip";
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter());
            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "questionnaire");
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_files_and_dont_have_main_file_should_return_empty_errors()
        {
            // arrange
            var arhiveFileName = "arhive.zip";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {Create.Entity.Roster(variable: "someRoster")}));

            var preloadedFile = Create.Entity.PreloadedFileInfo(questionnaireOrRosterName: "someroster");
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles(arhiveFileName, new[] { preloadedFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0040"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(arhiveFileName));
            Assert.That(errors[0].References.First().Content, Is.EqualTo("Questionnaire.tab"));
        }

        [Test]
        public void when_verify_columns_and_roster_file_without_roster_size_text_list_column_should_return_PL0052_error()
        {
            // arrange
            var roster = "myroster";
            Guid rosterSizeId = Guid.Parse("11111111111111111111111111111111");
            string textListQuestionVariable = "listrostersize";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(rosterSizeId, variable: textListQuestionVariable),
                    Create.Entity.Roster(variable: roster, rosterSizeQuestionId: rosterSizeId,
                        children: new[]
                        {
                            Create.Entity.TextQuestion()
                        })));

            var mainFile = Create.Entity.PreloadedFileInfo(new[] { ServiceColumns.InterviewId });
            var rosterFile = Create.Entity.PreloadedFileInfo(
                new[]
                {
                    ServiceColumns.InterviewId, $"{roster}__id"
                }, fileName: roster, questionnaireOrRosterName: roster);

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyFiles("original.zip", new[] { mainFile, rosterFile }, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0052"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(roster));
            Assert.That(errors[0].References.First().Content, Is.EqualTo(textListQuestionVariable));
        }
    }
}
