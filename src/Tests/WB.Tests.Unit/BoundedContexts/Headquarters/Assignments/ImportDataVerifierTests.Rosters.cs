using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_numeric_roster_with_roster_instance_codes_out_of_order_should_return_PL0053_error()
        {
            // arrange
            var numericRosterSize = Guid.NewGuid();
            var numericRoster = "hhroster";
            var numericRosterColumn = $"{numericRoster}__id";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(numericRosterSize),
                    Create.Entity.NumericRoster(variable: numericRoster, rosterSizeQuestionId: numericRosterSize,
                        children: new[] {Create.Entity.TextQuestion()})
                }));


            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow("Questionnaire", 1, "interview1"),
                Create.Entity.PreloadingAssignmentRow(numericRoster, 1, "interview1", Create.Entity.AssignmentRosterInstanceCode(numericRosterColumn, 0)),
                Create.Entity.PreloadingAssignmentRow(numericRoster, 2, "interview1", Create.Entity.AssignmentRosterInstanceCode(numericRosterColumn, 2)),
                Create.Entity.PreloadingAssignmentRow(numericRoster, 3, "interview1", Create.Entity.AssignmentRosterInstanceCode(numericRosterColumn, 3))
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors, Has.One.Items);
            Assert.That(errors[0].Code, Is.EqualTo("PL0053"));
            Assert.That(errors[0].References, Has.Exactly(2).Items);
            Assert.That(errors[0].References.ElementAt(0).Column, Is.EqualTo(numericRosterColumn));
            Assert.That(errors[0].References.ElementAt(0).Row, Is.EqualTo(2));
            Assert.That(errors[0].References.ElementAt(0).Content, Is.EqualTo("2"));
            Assert.That(errors[0].References.ElementAt(1).Column, Is.EqualTo(numericRosterColumn));
            Assert.That(errors[0].References.ElementAt(1).Row, Is.EqualTo(3));
            Assert.That(errors[0].References.ElementAt(1).Content, Is.EqualTo("3"));
        }

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
                Create.Entity.PreloadingAssignmentRow("main", 1, "interview1", "Questionnaire"),
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
                Create.Entity.PreloadingAssignmentRow("main", 1, "interview1", "Questionnaire"),
                Create.Entity.PreloadingAssignmentRow(parentRoster, 1, "interview1", 
                    Create.Entity.AssignmentRosterInstanceCode(parentRosterColumn, 1)
                ),
                Create.Entity.PreloadingAssignmentRow(nestedRoster, 1, "interview1", 
                    Create.Entity.AssignmentRosterInstanceCode(parentRosterColumn, 2),
                    Create.Entity.AssignmentRosterInstanceCode(nestedRosterColumn, 2)
                ),
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
        public void when_verify_main_file_with_orphan_nested_roster_should_return_PL0008_error()
        {
            // arrange
            var fileName = "Questionnaire.tab";
            var roster = "hhroster";
            var textInSection = "textInSection";
            var textInRoster = "textInRoster";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.TextQuestion(variable: textInSection),
                    Create.Entity.FixedRoster(variable: roster, fixedTitles: Create.Entity.FixedTitles(1, 2), children: new []
                    {
                        Create.Entity.TextQuestion(variable: textInRoster),
                    })
                })
            );

            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow(fileName, 1, "interviewId1", "Questionnaire"),
                Create.Entity.PreloadingAssignmentRow(roster, 1, "interviewId2", "roster",
                    Create.Entity.AssignmentRosterInstanceCode(ServiceColumns.IdSuffixFormat.FormatString(roster), 2)
                ),
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));

            var reference = errors[0].References.First();
            Assert.That(reference.Content, Is.EqualTo("interviewId2"));
            Assert.That(reference.Row, Is.EqualTo(1));
            Assert.That(reference.Column, Is.EqualTo(ServiceColumns.InterviewId));
            Assert.That(reference.DataFile, Is.EqualTo(roster));
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

        [Test]
        public void when_verify_nested_numeric_roster_with_roster_instance_codes_out_of_order_should_return_PL0053_error()
        {
            // arrange
            var interviewId = "interview1";

            var roster = "myroster";
            var nestedNumericRoster = "hhroster";

            var numericRosterSize = Guid.NewGuid();
            var rosterColumn = $"{roster}__id";
            var numericRosterColumn = $"{nestedNumericRoster}__id";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.FixedRoster(variable: roster, fixedTitles: Create.Entity.FixedTitles(10, 20), children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(numericRosterSize),
                        Create.Entity.NumericRoster(variable: nestedNumericRoster, rosterSizeQuestionId: numericRosterSize,
                            children: new[] {Create.Entity.TextQuestion()})
                    })
                }));


            var allRowsByAllFiles = new List<PreloadingAssignmentRow>
            {
                Create.Entity.PreloadingAssignmentRow("Questionnaire", 1, interviewId),
                Create.Entity.PreloadingAssignmentRow(roster, 1, interviewId, Create.Entity.AssignmentRosterInstanceCode(rosterColumn, 10)),
                Create.Entity.PreloadingAssignmentRow(roster, 2, interviewId, Create.Entity.AssignmentRosterInstanceCode(rosterColumn, 20)),
                Create.Entity.PreloadingAssignmentRow(nestedNumericRoster, 1, interviewId, Create.Entity.AssignmentRosterInstanceCode(rosterColumn, 10),
                    Create.Entity.AssignmentRosterInstanceCode(numericRosterColumn, 0)),
                Create.Entity.PreloadingAssignmentRow(nestedNumericRoster, 2, interviewId, Create.Entity.AssignmentRosterInstanceCode(rosterColumn, 20), 
                    Create.Entity.AssignmentRosterInstanceCode(numericRosterColumn, 2))
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors, Has.One.Items);
            Assert.That(errors[0].Code, Is.EqualTo("PL0053"));
            Assert.That(errors[0].References, Has.One.Items);
            Assert.That(errors[0].References.ElementAt(0).Column, Is.EqualTo(numericRosterColumn));
            Assert.That(errors[0].References.ElementAt(0).Row, Is.EqualTo(2));
            Assert.That(errors[0].References.ElementAt(0).Content, Is.EqualTo("2"));
        }
        
        [Test]
        public void when_verify_assignments_with_duplicated_interview_id_in_main_file_should_return_PL0006_error()
        {
            // arrange
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(variable: "txt")
                }));

            
            var allRowsByAllFiles = new List<PreloadingAssignmentRow>()
            {
                Create.Entity.PreloadingAssignmentRow("main", 1, "interview1", "Questionnaire"),
                Create.Entity.PreloadingAssignmentRow("main", 2, "interview2", "Questionnaire"),
                Create.Entity.PreloadingAssignmentRow("main", 3, "interview1", "Questionnaire"),
            };

            var verifier = Create.Service.ImportDataVerifier();

            // act
            var errors = verifier.VerifyRosters(allRowsByAllFiles, questionnaire).ToArray();

            // assert
            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0].Code, Is.EqualTo("PL0006"));
            Assert.That(errors[0].References.Count(), Is.EqualTo(2));

            Assert.That(errors[0].References.ElementAt(0).Content, Is.EqualTo("interview1"));
            Assert.That(errors[0].References.ElementAt(0).Row, Is.EqualTo(1));
            Assert.That(errors[0].References.ElementAt(0).Column, Is.EqualTo(ServiceColumns.InterviewId));
            Assert.That(errors[0].References.ElementAt(0).DataFile, Is.EqualTo("main"));

            Assert.That(errors[0].References.ElementAt(1).Content, Is.EqualTo("interview1"));
            Assert.That(errors[0].References.ElementAt(1).Row, Is.EqualTo(3));
            Assert.That(errors[0].References.ElementAt(1).Column, Is.EqualTo(ServiceColumns.InterviewId));
            Assert.That(errors[0].References.ElementAt(1).DataFile, Is.EqualTo("main"));
        }
    }
}
