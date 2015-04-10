using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_changing_roster_group_to_simple_group_but_question_inside_group_is_used_as_link_and_group_inside_other_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            var rosterFixedTitles = new[] {new Tuple<decimal, string>(1, "1"), new Tuple<decimal, string>(2, "2")};
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: parentRosterId){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = rosterFixedTitles,
                    RosterTitleQuestionId = null
                });


            questionnaire.Apply(new NewGroupAdded { PublicKey = groupToUpdateId, ParentGroupPublicKey = parentRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, groupToUpdateId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: groupToUpdateId)
            {
                RosterSizeQuestionId = null,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                FixedRosterTitles = rosterFixedTitles,
                RosterTitleQuestionId = null
            });


            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = questionUsedAsLinkId,
                GroupPublicKey = groupToUpdateId,
                IsInteger = true,
                MaxAllowedValue = 5
            });

            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = linkedQuestionInChapterId,
                QuestionType = QuestionType.SingleOption,
                GroupPublicKey = chapterId,
                LinkedToQuestionId = questionUsedAsLinkId
            });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of =
            () =>
                questionnaire.UpdateGroup(groupToUpdateId, responsibleId, "title", null, null, "", "", false, RosterSizeSourceType.Question, null, null);

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
