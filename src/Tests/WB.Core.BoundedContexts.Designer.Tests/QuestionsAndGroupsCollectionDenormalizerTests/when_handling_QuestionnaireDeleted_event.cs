using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_QuestionnaireDeleted_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            evnt = CreateQuestionnaireDeletedEvent();
            denormalizer = CreateQuestionnaireInfoDenormalizer();
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_view_with_is_deleted_set_in_true = () =>
            newState.IsDeleted.ShouldBeTrue();

        It should_return_not_null_questions_collection_in_result_view = () =>
            newState.Questions.ShouldBeEmpty();

        It should_return_not_null_groups_collection_in_result_sview = () =>
            newState.Groups.ShouldBeEmpty();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireDeleted> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
    }
}