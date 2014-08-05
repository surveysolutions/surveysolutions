using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_roster_group_by_question_and_roster_size_question_is_under_another_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            sourceGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                QuestionType = QuestionType.MultyOption,
                GroupPublicKey = anotherRosterId
            });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
                questionnaire.CloneGroupWithoutChildren(targetGroupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, anotherRosterId,
                    sourceGroupId, 0, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        It should_raise_GroupBecameARoster_event = () =>
            eventContext.ShouldContainEvent<GroupBecameARoster>();

        It should_raise_GroupBecameARoster_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupBecameARoster>()
                .GroupId.ShouldEqual(targetGroupId);

        It should_raise_RosterChanged_event = () =>
            eventContext.ShouldContainEvent<RosterChanged>();

        It should_raise_RosterChanged_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<RosterChanged>()
                .GroupId.ShouldEqual(targetGroupId);

        It should_raise_GroupCloned_event = () =>
         eventContext.ShouldContainEvent<GroupCloned>();

        It should_raise_GroupCloned_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupCloned>()
                .PublicKey.ShouldEqual(targetGroupId);


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid sourceGroupId;
        private static Guid rosterSizeQuestionId;
        private static Guid targetGroupId;
        private static EventContext eventContext;
        private static Guid chapterId;
        private static Guid anotherRosterId;
    }
}