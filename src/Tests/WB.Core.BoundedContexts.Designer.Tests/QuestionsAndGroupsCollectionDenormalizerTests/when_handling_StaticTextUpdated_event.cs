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
    internal class when_handling_StaticTextUpdated_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();

            questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            questionnaireEntityFactoryMock
                .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>()))
                .Returns((Guid entityId, string text) => new StaticText(entityId: entityId, text: text));

            evnt = CreateStaticTextUpdatedEvent(entityId:st2Id, text: newText);

            denormalizer = CreateQuestionnaireInfoDenormalizer(
                questionnaireEntityFactory: questionnaireEntityFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_static_text_collection_in_result_view = () =>
            newState.StaticTexts.ShouldNotBeNull();

        It should_return_2_items_in_static_text_collection = () =>
            newState.StaticTexts.Count.ShouldEqual(2);

        It should_return_static_text_N2_with_text_equals__to_newText = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).Text.ShouldEqual(newText);

        It should_return_static_text_N2_with_parent_id_equals_g3Id = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).ParentGroupId.ShouldEqual(g4Id);

        It should_return_static_text_N2_with_parent_group_ids_contains_only_g4Id_g2Id_g1Id = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).ParentGroupsIds.ShouldContainOnly(g4Id, g2Id, g1Id);

        It should_return_static_text_N2_with_roster_scope_ids_contains_only_q2Id = () =>
            newState.StaticTexts.Single(x => x.Id == st2Id).RosterScopeIds.ShouldContainOnly(q2Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<StaticTextUpdated> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactoryMock;
        private static string newText = "New text";
    }
}