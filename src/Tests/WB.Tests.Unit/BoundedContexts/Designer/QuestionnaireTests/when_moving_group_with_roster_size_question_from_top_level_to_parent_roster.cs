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
    internal class when_moving_group_with_roster_size_question_from_top_level_to_parent_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: parentRosterId){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles =  new[] { new Tuple<decimal, string>(1,"1"), new Tuple<decimal, string>(2,"2") },
                    RosterTitleQuestionId =null 
                });

            questionnaire.Apply(new NewGroupAdded { PublicKey = groupToMoveId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = groupToMoveId,
                IsInteger = true,
                MaxAllowedValue = 5
            });


            questionnaire.Apply(new NewGroupAdded { PublicKey = nestedRosterId, ParentGroupPublicKey = parentRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, nestedRosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: nestedRosterId){
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
