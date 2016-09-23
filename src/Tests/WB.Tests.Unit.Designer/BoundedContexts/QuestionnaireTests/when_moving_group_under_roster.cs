using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, GroupText = "group to move", ParentGroupPublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, parentRosterId));
        };

        Because of = () => questionnaire.MoveGroup(groupId, parentRosterId, targetIndex, responsibleId);


        It should_contains_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ShouldNotBeNull();

        It should_contains_group_with_GroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .PublicKey.ShouldEqual(groupId);

        It should_contains_group_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .GetParent().PublicKey.ShouldEqual(parentRosterId);

        It should_contains_group_with_TargetIndex_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId)
                .GetParent().Children[targetIndex].PublicKey.ShouldEqual(groupId);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static int targetIndex = 0;
    }
}