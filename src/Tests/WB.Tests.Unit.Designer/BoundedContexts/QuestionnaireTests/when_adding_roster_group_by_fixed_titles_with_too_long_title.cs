using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_by_fixed_titles_with_too_long_title : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: tooLongTitle, variableName: null,
                    rosterSizeQuestionId: null, description: null, condition: null, hideIfDisabled: false, parentGroupId: parentGroupId,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "roster fixed title 1"), new FixedRosterTitleItem("2", "roster fixed title 2") }, rosterTitleQuestionId: null));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__exist__ () =>
            (exception as QuestionnaireException).ErrorType.ShouldEqual(DomainExceptionType.TitleIsTooLarge);

        private static Questionnaire questionnaire;
        private static string tooLongTitle = "A".PadRight(501,'A');
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static Exception exception;
    }
}