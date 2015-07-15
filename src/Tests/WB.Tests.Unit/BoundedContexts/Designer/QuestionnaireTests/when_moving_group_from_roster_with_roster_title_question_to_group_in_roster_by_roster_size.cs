using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_roster_title_question_to_group_in_roster_by_roster_size_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = chapterId,
                IsInteger = true
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = targetRosterId, ParentGroupPublicKey = chapterId});
            questionnaire.Apply(new GroupBecameARoster(responsibleId, targetRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: targetRosterId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = null
                });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromTargetRosterId, ParentGroupPublicKey = targetRosterId });

            questionnaire.Apply(new NewGroupAdded { PublicKey = sourceRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, sourceRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: sourceRosterId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });

            questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromSourceRosterId, ParentGroupPublicKey = sourceRosterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = groupFromSourceRosterId
            });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.MoveGroup(groupFromSourceRosterId, groupFromTargetRosterId, 0, responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .PublicKey.ShouldEqual(groupFromSourceRosterId);

        It should_raise_QuestionnaireItemMoved_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .GroupKey.ShouldEqual(groupFromTargetRosterId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid targetRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid sourceRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupFromSourceRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid groupFromTargetRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterTitleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static EventContext eventContext;
    }
}