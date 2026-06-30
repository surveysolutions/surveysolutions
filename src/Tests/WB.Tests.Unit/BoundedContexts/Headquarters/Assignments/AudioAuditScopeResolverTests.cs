using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestFixture]
    [TestOf(typeof(AudioAuditScopeResolver))]
    public class AudioAuditScopeResolverTests
    {
        private static readonly Guid SectionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid GroupId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid RosterId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid QuestionId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid FlatRosterId = Guid.Parse("55555555555555555555555555555555");

        private static IQuestionnaire CreateQuestionnaire()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId: SectionId,
                children: new[]
                {
                    Create.Entity.Group(groupId: GroupId, variable: "household",
                        children: new[]
                        {
                            Create.Entity.Roster(rosterId: RosterId, variable: "members_roster",
                                children: new[]
                                {
                                    Create.Entity.TextQuestion(questionId: QuestionId, variable: "first_name")
                                }),
                            Create.Entity.Roster(rosterId: FlatRosterId, variable: "flat_roster",
                                displayMode: RosterDisplayMode.Flat,
                                fixedTitles: new[] { "1", "2" })
                        })
                });

            return Create.Entity.PlainQuestionnaire(questionnaireDocument);
        }

        [Test]
        public void should_resolve_section_group_and_roster_variable_names()
        {
            var questionnaire = CreateQuestionnaire();
            var sectionVariable = questionnaire.GetRosterVariableName(SectionId);

            var result = AudioAuditScopeResolver.Resolve(questionnaire,
                new[] { sectionVariable, "household", "members_roster" });

            result.HasErrors.Should().BeFalse();
            result.VariableNames.Should().BeEquivalentTo(new[] { sectionVariable, "household", "members_roster" });
        }

        [Test]
        public void should_report_unknown_variable_name_as_invalid()
        {
            var questionnaire = CreateQuestionnaire();

            var result = AudioAuditScopeResolver.Resolve(questionnaire, new[] { "household", "does_not_exist" });

            result.HasErrors.Should().BeTrue();
            result.InvalidVariableNames.Should().BeEquivalentTo(new[] { "does_not_exist" });
            result.VariableNames.Should().BeEquivalentTo(new[] { "household" });
        }

        [Test]
        public void should_report_question_variable_name_as_invalid()
        {
            var questionnaire = CreateQuestionnaire();

            var result = AudioAuditScopeResolver.Resolve(questionnaire, new[] { "first_name" });

            result.HasErrors.Should().BeTrue();
            result.InvalidVariableNames.Should().BeEquivalentTo(new[] { "first_name" });
            result.VariableNames.Should().BeEmpty();
        }

        [Test]
        public void should_report_flat_roster_variable_name_as_invalid()
        {
            var questionnaire = CreateQuestionnaire();

            var result = AudioAuditScopeResolver.Resolve(questionnaire, new[] { "household", "flat_roster" });

            result.HasErrors.Should().BeTrue();
            result.InvalidVariableNames.Should().BeEquivalentTo(new[] { "flat_roster" });
            result.VariableNames.Should().BeEquivalentTo(new[] { "household" });
        }

        [Test]
        public void should_return_empty_result_for_empty_input()
        {
            var questionnaire = CreateQuestionnaire();

            var result = AudioAuditScopeResolver.Resolve(questionnaire, Array.Empty<string>());

            result.HasErrors.Should().BeFalse();
            result.VariableNames.Should().BeEmpty();
        }

        [Test]
        public void should_ignore_blank_names_and_deduplicate()
        {
            var questionnaire = CreateQuestionnaire();

            var result = AudioAuditScopeResolver.Resolve(questionnaire,
                new[] { "household", " ", "household", null });

            result.HasErrors.Should().BeFalse();
            result.VariableNames.Should().BeEquivalentTo(new[] { "household" });
        }
    }
}
