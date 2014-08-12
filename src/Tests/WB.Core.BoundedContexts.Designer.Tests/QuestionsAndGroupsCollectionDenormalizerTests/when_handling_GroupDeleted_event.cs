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
    internal class when_handling_GroupDeleted_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            evnt = CreateGroupDeletedEvent(g2Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_groups_collection_in_result_view = () =>
            newState.Groups.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_2_items_in_groups_collection = () =>
            newState.Groups.Count.ShouldEqual(2);

        It should_return_items_in_groups_collection_with_specified_ids = () =>
            newState.Groups.Select(x => x.Id).ShouldContainOnly(g1Id, g5Id);

        It should_return_3_items_in_questions_collection = () =>
            newState.Questions.Count.ShouldEqual(3);

        It should_return_items_in_questions_collection_with_specified_ids = () =>
            newState.Questions.Select(x => x.Id).ShouldContainOnly(q1Id, q2Id, q6Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<GroupDeleted> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}