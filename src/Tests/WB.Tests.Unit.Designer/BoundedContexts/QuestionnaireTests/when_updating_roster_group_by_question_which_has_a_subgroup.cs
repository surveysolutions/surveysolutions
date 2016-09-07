using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_roster_group_by_question_which_has_a_subgroup : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(publicKey: rosterSizeQuestionId, isInteger: true, groupPublicKey: chapterId));
            
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId});
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = subGroupId, ParentGroupPublicKey = groupId });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.UpdateGroup(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, hideIfDisabled: false, isRoster: true,
                    rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupUpdated_event = () =>
            eventContext.ShouldContainEvent<GroupUpdated>();

        It should_raise_GroupUpdated_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<GroupUpdated>()
                .GroupPublicKey.ShouldEqual(groupId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid subGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static EventContext eventContext;
    }
}