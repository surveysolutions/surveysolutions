using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_roster_group_by_fixed_titles_with_too_long_title : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: tooLongTitle, variableName: null,
                    rosterSizeQuestionId: null, description: null, condition: null, parentGroupId: parentGroupId,
                    isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles, rosterFixedTitles: new[] {new Tuple<string, string>("1","roster fixed title 1"),new Tuple<string, string>("2", "roster fixed title 2") }, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question__exist__ = () =>
            (exception as QuestionnaireException).ErrorType.ShouldEqual(DomainExceptionType.TitleIsTooLarge);

        private static Questionnaire questionnaire;
        private static string tooLongTitle = "A".PadRight(501,'A');
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static Exception exception;
    }
}