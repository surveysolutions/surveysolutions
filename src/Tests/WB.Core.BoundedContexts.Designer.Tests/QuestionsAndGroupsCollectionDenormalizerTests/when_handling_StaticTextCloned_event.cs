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
    internal class when_handling_StaticTextCloned_event : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();

            questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            questionnaireEntityFactoryMock
                .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>()))
                .Returns((Guid entityId, string text) => new StaticText(entityId, text));

            evnt = CreateStaticTextClonedEvent(entityId: newStaticTextId, parentId: g3Id, sourceEntityId: st1Id);

            denormalizer = CreateQuestionnaireInfoDenormalizer(questionnaireEntityFactory: questionnaireEntityFactoryMock.Object);
        };

        Because of = () =>
            newState = denormalizer.Update(previousState, evnt);

        It should_return_not_null_view = () =>
            newState.ShouldNotBeNull();

        It should_return_not_null_static_text_collection_in_result_view = () =>
            newState.StaticTexts.ShouldNotBeNull();

        It should_return_3_items_in_static_text_collection = () =>
            newState.StaticTexts.Count.ShouldEqual(3);

        It should_return_cloned_static_text_with_parent_id_equals_g3Id = () =>
            newState.StaticTexts.Single(x => x.Id == newStaticTextId).ParentGroupId.ShouldEqual(g3Id);

        It should_return_cloned_static_text_with_parent_group_ids_contains_only_g3Id_g2Id_g1Id = () =>
            newState.StaticTexts.Single(x => x.Id == newStaticTextId).ParentGroupsIds.ShouldContainOnly(g3Id, g2Id, g1Id);

        It should_return_cloned_static_text_with_roster_scope_ids_contains_only_g3Id_q2Id = () =>
            newState.StaticTexts.Single(x => x.Id == newStaticTextId).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<StaticTextCloned> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactoryMock;
        private static Guid newStaticTextId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}