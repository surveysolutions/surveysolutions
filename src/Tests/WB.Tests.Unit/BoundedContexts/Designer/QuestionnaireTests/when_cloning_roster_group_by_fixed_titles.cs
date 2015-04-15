using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_fixed_titles : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            sourceGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            rosterSizeSourceType = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { new FixedRosterTitleItem("1", rosterFixedTitle1), 
                new FixedRosterTitleItem("2", rosterFixedTitle2) };

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = Guid.NewGuid(),
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.Text
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = sourceGroupId, ParentGroupPublicKey = parentGroupId });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.CloneGroupWithoutChildren(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                parentGroupId: parentGroupId, description: null, condition: null, rosterSizeQuestionId: null, isRoster: true,
                rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: null, targetIndex: 0,
                sourceGroupId: sourceGroupId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupBecameARoster_event = () =>
            eventContext.ShouldContainEvent<GroupBecameARoster>();

        It should_raise_GroupBecameARoster_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupBecameARoster>()
                .GroupId.ShouldEqual(groupId);

        It should_raise_RosterChanged_event = () =>
            eventContext.ShouldContainEvent<RosterChanged>();

        It should_raise_RosterChanged_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .GroupId.ShouldEqual(groupId);

        It should_raise_RosterChanged_event_with_RosterSizeSourceType_equal_to_specified_rosterSizeSourceType = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .RosterSizeSource.ShouldEqual(rosterSizeSourceType);

        It should_raise_RosterChanged_event_with_RosterSizeQuestionId_equal_to_null = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterSizeQuestionId.ShouldBeNull();

        It should_raise_RosterChanged_event_with_not_nullable_RosterFixedTitles = () =>
            eventContext.GetSingleEvent<RosterChanged>().FixedRosterTitles.ShouldNotBeNull();

        It should_raise_RosterChanged_event_with_RosterFixedTitles_that_have_2_items = () =>
            eventContext.GetSingleEvent<RosterChanged>().FixedRosterTitles.ShouldNotBeEmpty();

        It should_raise_RosterChanged_event_with_RosterFixedTitles_that_first_element_is_specified = () =>
            eventContext.GetSingleEvent<RosterChanged>().FixedRosterTitles[0].Title.ShouldEqual(rosterFixedTitle1);

        It should_raise_RosterChanged_event_with_RosterFixedTitles_that_second_element_is_specified = () =>
            eventContext.GetSingleEvent<RosterChanged>().FixedRosterTitles[1].Title.ShouldEqual(rosterFixedTitle2);

        It should_raise_RosterChanged_event_with_RosterTitleQuestionId_equal_to_null = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterTitleQuestionId.ShouldBeNull();

        It should_raise_RosterChanged_event_with_RosteTitleQuestionId_equal_to_null = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterTitleQuestionId.ShouldBeNull();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        private static Guid sourceGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static FixedRosterTitleItem[] rosterFixedTitles;
        private static string rosterFixedTitle1 = "roster fixed title 1";
        private static string rosterFixedTitle2 = "roster fixd title 2";
    }
}