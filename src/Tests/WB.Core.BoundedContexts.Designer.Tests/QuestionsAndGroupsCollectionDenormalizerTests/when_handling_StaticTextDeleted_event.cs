using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_StaticTextDeleted_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();

            questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            questionnaireEntityFactoryMock
                .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>()))
                .Returns((Guid entityId, string text) => new StaticText(entityId, text));

            evnt = CreateStaticTextDeletedEvent(entityId: st1Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(questionnaireEntityFactory: questionnaireEntityFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_static_text_collection_in_result_view = () =>
            newState.StaticTexts.ShouldNotBeNull();

        It should_static_text_does_not_contains_in_static_text_collection_in_result_view = () =>
            newState.StaticTexts.FirstOrDefault(x => x.Id == st1Id).ShouldBeNull();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<StaticTextDeleted> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactoryMock;
    }
}