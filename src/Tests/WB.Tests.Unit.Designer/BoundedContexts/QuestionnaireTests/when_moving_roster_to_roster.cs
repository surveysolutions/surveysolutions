using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = roster1Id, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster1Id));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId, roster1Id){
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new[] { new FixedRosterTitle(1, "test"), new FixedRosterTitle(2, "test 2") },
                    RosterTitleQuestionId = null 
                });
            
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = roster2Id, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster2Id));
        };


        Because of = () =>
                questionnaire.MoveGroup(roster1Id, roster2Id, 0, responsibleId);

        private It should_raise_QuestionnaireItemMoved_event = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(roster1Id).ShouldNotBeNull();

        It should_raise_QuestionnaireItemMoved_event_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(roster1Id)
           .PublicKey.ShouldEqual(roster1Id);

        It should_raise_QuestionnaireItemMoved_event_with_roster2Id_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(roster1Id)
            .GetParent().PublicKey.ShouldEqual(roster2Id);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}