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
    internal class when_handling_QuestionnaireItemMoved_event_and_group_is_moving_into_its_child : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsViewMapperMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            evnt = CreateQuestionnaireItemMovedEvent(g2Id, g4Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsViewMapperMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_6_items_in_questions_collection = () =>
            newState.Groups.Count.ShouldEqual(5);

        It should_return_question_N4_with_parent_id_equals_g4Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).ParentGroupId.ShouldEqual(g4Id);

        It should_return_question_N4_with_parent_group_ids_contains_only_g4Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).ParentGroupsIds.ShouldContainOnly(g4Id);

        It should_return_question_N4_with_roster_scope_ids_contains_only_q2Id = () =>
            newState.Groups.Single(x => x.Id == g2Id).RosterScopeIds.ShouldContainOnly(q2Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsViewMapperMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}