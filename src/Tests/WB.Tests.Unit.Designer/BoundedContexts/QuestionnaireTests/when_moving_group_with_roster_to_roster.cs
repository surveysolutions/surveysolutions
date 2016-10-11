using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);
            questionnaire.AddGroup(roster1Id, groupId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster1Id));
            questionnaire.AddGroup(roster2Id, chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, roster2Id));
        };


        Because of = () =>
            questionnaire.MoveGroup(groupId, roster2Id, 0, responsibleId);

        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
           .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_roster2Id_specified = () =>
           questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
            .GetParent().PublicKey.ShouldEqual(roster2Id);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}