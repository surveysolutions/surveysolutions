using System.Linq;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        private static WB.Core.SharedKernels.DataCollection.Aggregates.IQuestionnaire QuestionnaireWithSectionGroupRosterAndQuestion()
            => Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.Group(title: "Household", variable: "household", children: new[]
                    {
                        Create.Entity.TextQuestion(variable: "name")
                    }),
                    Create.Entity.Roster(title: "Members", variable: "members_roster")
                }));

        [Test]
        public void when_verify_audio_recording_scope_referencing_group_and_roster_should_not_return_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = QuestionnaireWithSectionGroupRosterAndQuestion();

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                assignmentAudioRecordingScope: Create.Entity.AssignmentAudioRecordingScope("household", "members_roster"));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_audio_recording_scope_with_empty_scope_should_not_return_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = QuestionnaireWithSectionGroupRosterAndQuestion();

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                assignmentAudioRecordingScope: Create.Entity.AssignmentAudioRecordingScope());
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void when_verify_audio_recording_scope_referencing_unknown_variable_should_return_PL0064_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = QuestionnaireWithSectionGroupRosterAndQuestion();

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                assignmentAudioRecordingScope: Create.Entity.AssignmentAudioRecordingScope("not_existing"));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0064"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo("not_existing"));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(fileName));
        }

        [Test]
        public void when_verify_audio_recording_scope_referencing_question_should_return_PL0065_error()
        {
            // arrange
            var fileName = "mainfile.tab";
            var questionnaire = QuestionnaireWithSectionGroupRosterAndQuestion();

            var preloadingRow = Create.Entity.PreloadingAssignmentRow(fileName,
                assignmentAudioRecordingScope: Create.Entity.AssignmentAudioRecordingScope("name"));
            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRowValues(preloadingRow, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0065"));
            Assert.That(errors[0].References.First().Content, Is.EqualTo("name"));
        }
    }
}
