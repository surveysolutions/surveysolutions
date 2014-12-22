using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
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
            newState.ShouldBeNull();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireDeleted> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
    }
}