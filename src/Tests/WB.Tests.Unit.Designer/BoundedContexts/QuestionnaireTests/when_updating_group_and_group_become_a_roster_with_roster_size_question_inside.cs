using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_group_become_a_roster_with_roster_size_question_inside : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(
                publicKey : rosterSizeQuestionId,
                groupPublicKey : groupId,
                isInteger : true
            ));
            
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: rosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =null 
                });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
                questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null, rosterSizeQuestionId: rosterSizeQuestionId,
                    description: null, condition: null, hideIfDisabled: false, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question,
                    rosterFixedTitles: null, rosterTitleQuestionId: null);

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

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}