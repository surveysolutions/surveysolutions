using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_roster_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster1Id, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster1Id));
            questionnaire.Apply(new RosterChanged(responsibleId, roster1Id){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new[] { new FixedRosterTitle(1, "test") },
                    RosterTitleQuestionId = null 
                });
            
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster2Id, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster2Id));
            
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
                questionnaire.MoveGroup(roster1Id, roster2Id, 0, responsibleId);

        It should_raise_QuestionnaireItemMoved_event = () =>
          eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
           .PublicKey.ShouldEqual(roster1Id);

        It should_raise_QuestionnaireItemMoved_event_with_roster2Id_specified = () =>
          eventContext.GetSingleEvent<QuestionnaireItemMoved>()
            .GroupKey.ShouldEqual(roster2Id);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static EventContext eventContext;
    }
}