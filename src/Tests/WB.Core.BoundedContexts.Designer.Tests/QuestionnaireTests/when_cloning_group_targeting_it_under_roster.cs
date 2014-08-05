using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_group_targeting_it_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = sourceGroupId, GroupText = "Group to be cloned", ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneGroupWithoutChildren(targetGroupId, responsibleId, "title",null, null, null, null, parentRosterId,
                    sourceGroupId, 0, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupCloned_event = () =>
            eventContext.ShouldContainEvent<GroupCloned>();

        It should_raise_GroupCloned_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupCloned>()
                .PublicKey.ShouldEqual(targetGroupId);

        It should_raise_GroupCloned_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<GroupCloned>()
                .ParentGroupPublicKey.ShouldEqual(parentRosterId);

        private static Questionnaire questionnaire;
        private static Guid targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid sourceGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static EventContext eventContext;
    }
}