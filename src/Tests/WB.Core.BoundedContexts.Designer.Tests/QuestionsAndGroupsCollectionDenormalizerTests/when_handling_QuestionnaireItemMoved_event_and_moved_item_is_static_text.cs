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
    internal class when_handling_QuestionnaireItemMoved_event_and_moved_item_is_static_text : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            
            evnt = CreateQuestionnaireItemMovedEvent(st2Id, g1Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_static_text_collection_in_result_view = () =>
            newState.StaticTexts.ShouldNotBeNull();

        It should_return_6_items_in_static_text_collection = () =>
            newState.StaticTexts.Count.ShouldEqual(2);

        It should_return_static_text_N2_with_parent_id_equals_g1Id = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).ParentGroupId.ShouldEqual(g1Id);

        It should_return_static_text_N2_with_parent_group_ids_contains_only_g1Id = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).ParentGroupsIds.ShouldContainOnly(g1Id);

        It should_return_static_text_N2_with_empty_roster_scope_ids = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).RosterScopeIds.ShouldBeEmpty();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}