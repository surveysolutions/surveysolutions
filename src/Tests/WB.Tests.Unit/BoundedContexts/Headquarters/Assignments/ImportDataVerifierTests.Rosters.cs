using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_rosters_with_duplicated_roster_instances_should_return_PL0006_error()
        {
            // arrange
            var roster = "hhroster";
            var rosterInstanceColumn = $"{roster}__id";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.FixedRoster(variable: roster, fixedTitles: Create.Entity.FixedTitles(1,2))
                }));

            
            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow(roster, 1, "interview1", Create.Entity.AssignmentRosterInstanceCode(rosterInstanceColumn, 1)),
                Create.Entity.PreloadingAssignmentRow(roster, 2, "interview1", Create.Entity.AssignmentRosterInstanceCode(rosterInstanceColumn, 2)),
                Create.Entity.PreloadingAssignmentRow(roster, 3, "interview1", Create.Entity.AssignmentRosterInstanceCode(rosterInstanceColumn, 1))
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0006"));
            Assert.That(errors[0].References.Count(), Is.EqualTo(2));

            Assert.That(errors[0].References.ElementAt(0).Content, Is.EqualTo(1.ToString()));
            Assert.That(errors[0].References.ElementAt(0).Row, Is.EqualTo(1));
            Assert.That(errors[0].References.ElementAt(0).Column, Is.EqualTo(rosterInstanceColumn));
            Assert.That(errors[0].References.ElementAt(0).DataFile, Is.EqualTo(roster));

            Assert.That(errors[0].References.ElementAt(1).Content, Is.EqualTo(1.ToString()));
            Assert.That(errors[0].References.ElementAt(1).Row, Is.EqualTo(3));
            Assert.That(errors[0].References.ElementAt(1).Column, Is.EqualTo(rosterInstanceColumn));
            Assert.That(errors[0].References.ElementAt(1).DataFile, Is.EqualTo(roster));
        }

        [Test]
        public void when_verify_rosters_with_orphan_nested_roster_should_return_PL0008_error()
        {
            // arrange
            var parentRoster = "hhroster";
            var nestedRoster = "nested";

            var parentRosterColumn = $"{parentRoster}__id";
            var nestedRosterColumn = $"{nestedRoster}__id";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.FixedRoster(variable: parentRoster, fixedTitles: Create.Entity.FixedTitles(1, 2),
                        children: new[]
                            {Create.Entity.FixedRoster(variable: nestedRoster, fixedTitles: Create.Entity.FixedTitles(1, 2))})
                }));


            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow(parentRoster, 1, "interview1", Create.Entity.AssignmentRosterInstanceCode(parentRosterColumn, 1)),
                Create.Entity.PreloadingAssignmentRow(nestedRoster, 1, "interview1", Create.Entity.AssignmentRosterInstanceCode(parentRosterColumn, 2),
                    Create.Entity.AssignmentRosterInstanceCode(nestedRosterColumn, 2)),
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));

            Assert.That(errors[0].References.First().Content, Is.EqualTo(2.ToString()));
            Assert.That(errors[0].References.First().Row, Is.EqualTo(1));
            Assert.That(errors[0].References.First().Column, Is.EqualTo(parentRosterColumn));
            Assert.That(errors[0].References.First().DataFile, Is.EqualTo(nestedRoster));
        }

        [Test]
        public void when_verify_rosters_with_main_file_only_without_interview_id_column_should_return_empty_errors()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.FixedRoster(fixedTitles: Create.Entity.FixedTitles(1, 2),
                        children: new[]
                            {Create.Entity.FixedRoster(fixedTitles: Create.Entity.FixedTitles(1, 2))})
                }));


            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow("questionnaire", 1, null)
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors, Is.Empty);
        }
    }
}
