using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.CriticalRules;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_in_critical_rules : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            // Rule where all three fields match
            questionnaire.AddCriticalRule(new AddCriticalRule(questionnaire.Id, criticalRuleAllFieldsId, responsibleId));
            questionnaire.UpdateCriticalRule(new UpdateCriticalRule(questionnaire.Id, criticalRuleAllFieldsId,
                $"message {searchFor}", $"expression {searchFor}", $"description {searchFor}", responsibleId));

            // Rule where only expression matches
            questionnaire.AddCriticalRule(new AddCriticalRule(questionnaire.Id, criticalRuleExpressionOnlyId, responsibleId));
            questionnaire.UpdateCriticalRule(new UpdateCriticalRule(questionnaire.Id, criticalRuleExpressionOnlyId,
                "no match here", $"expression only {searchFor}", "no match here", responsibleId));

            command = Create.Command.ReplaceTextsCommand(searchFor, replaceWith, matchCase: true, userId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.ReplaceTexts(command);

        [NUnit.Framework.Test] public void should_replace_expression_of_critical_rule () =>
            questionnaire.QuestionnaireDocument.CriticalRules
                .Find(x => x.Id == criticalRuleAllFieldsId).Expression
                .Should().Be($"expression {replaceWith}");

        [NUnit.Framework.Test] public void should_replace_message_of_critical_rule () =>
            questionnaire.QuestionnaireDocument.CriticalRules
                .Find(x => x.Id == criticalRuleAllFieldsId).Message
                .Should().Be($"message {replaceWith}");

        [NUnit.Framework.Test] public void should_replace_description_of_critical_rule () =>
            questionnaire.QuestionnaireDocument.CriticalRules
                .Find(x => x.Id == criticalRuleAllFieldsId).Description
                .Should().Be($"description {replaceWith}");

        [NUnit.Framework.Test] public void should_replace_expression_when_only_expression_matches () =>
            questionnaire.QuestionnaireDocument.CriticalRules
                .Find(x => x.Id == criticalRuleExpressionOnlyId).Expression
                .Should().Be($"expression only {replaceWith}");

        [NUnit.Framework.Test] public void should_not_replace_non_matching_message () =>
            questionnaire.QuestionnaireDocument.CriticalRules
                .Find(x => x.Id == criticalRuleExpressionOnlyId).Message
                .Should().Be("no match here");

        [NUnit.Framework.Test] public void should_count_affected_entries_once_per_critical_rule () =>
            questionnaire.GetLastReplacedEntriesCount().Should().Be(2);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid criticalRuleAllFieldsId = Guid.Parse("77777777777777777777777777777777");
        static readonly Guid criticalRuleExpressionOnlyId = Guid.Parse("88888888888888888888888888888888");
        private static ReplaceTextsCommand command;
        private static Guid responsibleId;
        const string replaceWith = "replaced";
        const string searchFor = "to_replace";
    }
}
