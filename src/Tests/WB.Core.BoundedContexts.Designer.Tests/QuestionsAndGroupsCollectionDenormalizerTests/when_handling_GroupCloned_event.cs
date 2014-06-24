using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_GroupCloned_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionFactory>();

            evnt = CreateGroupClonedEvent(groupId, g3Id, enablementCondition, description, title);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Groups.ShouldNotBeNull();

        It should_return_6_items_in_groups_collection = () =>
            newState.Groups.Count.ShouldEqual(6);

        It should_return_group_N6_with_parent_id_equals_g3Id = () =>
            newState.Groups.Single(x => x.Id == groupId).ParentGroupId.ShouldEqual(g3Id);

        It should_return_group_N6_with_enablement_condition_equals_enablementCondition = () =>
            newState.Groups.Single(x => x.Id == groupId).EnablementCondition.ShouldEqual(enablementCondition);

        It should_return_group_N6_with_description_equals_description = () =>
            newState.Groups.Single(x => x.Id == groupId).Description.ShouldEqual(description);

        It should_return_group_N6_with_title_equals_title = () =>
            newState.Groups.Single(x => x.Id == groupId).Title.ShouldEqual(title);

        It should_return_group_N6_with_parent_group_ids_contains_only_g3Id_g2Id_g1Id = () =>
            newState.Groups.Single(x => x.Id == groupId).ParentGroupsIds.ShouldContainOnly(g3Id, g2Id, g1Id);

        It should_return_group_N6_with_roster_scope_ids_contains_only_g3Id_q2Id = () =>
            newState.Groups.Single(x => x.Id == groupId).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<GroupCloned> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionFactory> questionFactoryMock;
        private static Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static string enablementCondition = "expression";
        private static string description = "Description";
        private static string title = "New Group X";
    }
}