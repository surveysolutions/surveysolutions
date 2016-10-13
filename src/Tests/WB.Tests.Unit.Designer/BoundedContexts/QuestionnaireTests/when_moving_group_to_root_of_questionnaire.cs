using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_to_root_of_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId,chapterId, responsibleId: responsibleId);
            questionnaire.AddGroup(groupInGroupId, groupId, responsibleId: responsibleId);
        };

        Because of = () => questionnaire.MoveGroup(groupInGroupId, null, 0, responsibleId);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupInGroupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupInGroupId)
                .PublicKey.ShouldEqual(groupInGroupId);

        It should_contains_group_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupInGroupId)
                .GetParent().PublicKey.ShouldEqual(questionnaire.QuestionnaireDocument.PublicKey);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupInGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}