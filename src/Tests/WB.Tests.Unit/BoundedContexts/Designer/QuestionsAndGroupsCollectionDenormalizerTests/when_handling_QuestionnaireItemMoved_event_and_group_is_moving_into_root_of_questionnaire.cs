using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_QuestionnaireItemMoved_event_and_group_is_moving_into_root_of_questionnaire : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsViewMapperMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            evnt = CreateQuestionnaireItemMovedEvent(g1Id, null);

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

        It should_return_group_g1Id_with_parent_id_equal_event_source_id = () =>
            newState.Groups.Single(x => x.Id == g1Id).ParentGroupId.ShouldEqual(evnt.EventSourceId);

        It should_return_group_g1Id_with_empty_list_of_parents = () =>
            newState.Groups.Single(x => x.Id == g1Id).ParentGroupsIds.ShouldBeEmpty();

        It should_return_group_g1Id_with_empty_list_of_roster_scopes = () =>
            newState.Groups.Single(x => x.Id == g1Id).RosterScopeIds.ShouldBeEmpty();

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsViewMapperMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}
