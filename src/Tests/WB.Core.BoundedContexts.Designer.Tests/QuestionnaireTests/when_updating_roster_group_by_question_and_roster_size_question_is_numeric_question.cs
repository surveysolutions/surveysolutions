using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_and_roster_size_question_is_numeric_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded { PublicKey = rosterSizeQuestionId, IsInteger = true, GroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });

            eventContext = new EventContext();
        };

        private Because of = () =>
            questionnaire.UpdateGroup(groupId, responsibleId, "title", rosterSizeQuestionId, null, null, isRoster: true,
                rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null, rosterTitleQuestionId: rosterTitleQuestionId);

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

        It should_raise_RosterChanged_event_with_RosterSizeQuestionId_equal_to_specified_question_id = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_raise_RosterChanged_event_with_RosterFixedTitles_equal_to_null = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterFixedTitles.ShouldBeNull();

        It should_raise_RosterChanged_event_with_RosterTitleQuestionId_equal_to_rosterTitleQuestionId = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .RosterTitleQuestionId.ShouldEqual(rosterTitleQuestionId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static RosterSizeSourceType rosterSizeSourceType;
    }
}