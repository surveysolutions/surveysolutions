using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_NewQuestionnaireCreated_event : QuestionsAndGroupsCollectionDenormalizerTestContext
    {
        Establish context = () =>
        {
            evnt = CreateNewQuestionnaireCreatedEvent();
            denormalizer = CreateQuestionnaireInfoDenormalizer();
        };

        Because of = () =>
            newState = denormalizer.Create(evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldNotBeNull();

        It should_return_not_null_groups_collection_in_result_sview = () =>
            newState.Groups.ShouldNotBeNull();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<NewQuestionnaireCreated> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
    }
}