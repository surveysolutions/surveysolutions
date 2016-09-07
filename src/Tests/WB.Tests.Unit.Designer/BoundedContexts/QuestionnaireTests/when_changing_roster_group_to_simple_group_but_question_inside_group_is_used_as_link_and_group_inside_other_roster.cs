using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_changing_roster_group_to_simple_group_but_question_inside_group_is_used_as_link_and_group_inside_other_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            var rosterFixedTitles = new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") };
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.AddGroup(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, parentRosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: parentRosterId){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = rosterFixedTitles,
                    RosterTitleQuestionId = null
                });


            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupToUpdateId, ParentGroupPublicKey = parentRosterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, groupToUpdateId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: groupToUpdateId)
            {
                RosterSizeQuestionId = null,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                FixedRosterTitles = rosterFixedTitles,
                RosterTitleQuestionId = null
            });


            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(
                publicKey : questionUsedAsLinkId,
                groupPublicKey : groupToUpdateId,
                isInteger : true
            ));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : linkedQuestionInChapterId,
                questionType : QuestionType.SingleOption,
                groupPublicKey : chapterId,
                linkedToQuestionId : questionUsedAsLinkId
            ));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of =
            () =>
                questionnaire.UpdateGroup(groupToUpdateId, responsibleId, "title", null, null, "", "", false, false, RosterSizeSourceType.Question, null, null);

        It should_raise_GroupStoppedBeingARoster_event_with_groupToUpdateId_specified = () =>
            eventContext.GetSingleEvent<GroupStoppedBeingARoster>().GroupId.ShouldEqual(groupToUpdateId);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid parentRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid questionUsedAsLinkId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupToUpdateId = Guid.Parse("21111111111111111111111111111111");
        private static Guid linkedQuestionInChapterId = Guid.Parse("31111111111111111111111111111111");
        private static EventContext eventContext;
    }
}
