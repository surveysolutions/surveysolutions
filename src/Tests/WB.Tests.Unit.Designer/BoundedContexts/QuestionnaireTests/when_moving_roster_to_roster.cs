using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(roster1Id, chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.FixedTitles,
                rosterSizeQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "test"), new FixedRosterTitleItem("2", "test 2") });
            
            questionnaire.AddGroup(roster2Id, chapterId, responsibleId: responsibleId);
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