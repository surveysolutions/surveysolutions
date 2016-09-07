using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_size_question_from_top_level_to_parent_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, parentRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: parentRosterId){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") },
                    RosterTitleQuestionId = null 
                });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupToMoveId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(
                publicKey : rosterSizeQuestionId,
                groupPublicKey : groupToMoveId,
                isInteger : true
            ));

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = nestedRosterId, ParentGroupPublicKey = parentRosterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, nestedRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: nestedRosterId){
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

        Because of = () => questionnaire.MoveGroup(groupToMoveId, parentRosterId, 0, responsibleId);

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_parentRosterId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().GroupKey.ShouldEqual(parentRosterId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid parentRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupToMoveId = Guid.Parse("21111111111111111111111111111111");
        private static Guid nestedRosterId = Guid.Parse("31111111111111111111111111111111");
        private static EventContext eventContext;
    }
}
