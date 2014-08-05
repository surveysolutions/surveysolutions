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
    internal class when_handling_QuestionDeleted_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            evnt = CreateQuestionDeletedEvent(q3Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionDetailsViewMapper: questionDetailsFactoryMock.Object,
                questionnaireEntityFactory: questionFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_5_items_in_questions_collection = () =>
            newState.Questions.Count.ShouldEqual(5);

        It should_return_question_N7_with_parent_id_equals_g3Id = () =>
            newState.Questions.FirstOrDefault(x => x.Id == q3Id).ShouldBeNull();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionDeleted> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}