using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
{
    internal class when_handling_QuestionnaireItemMoved_event_and_moved_item_is_group : QuestionsAndGroupsCollectionViewInitializer
    {
        Establish context = () =>
        {
            InitializePreviousState();
            questionDetailsFactoryMock = new Mock<IQuestionDetailsViewMapper>();
            questionFactoryMock = new Mock<IQuestionnaireEntityFactory>();

            evnt = CreateQuestionnaireItemMovedEvent(g2Id, g5Id);

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

        It should_return_2nd_group_with_is_roster_set_in_true = () =>
            GetGroup(g2Id).IsRoster.ShouldBeTrue();

        It should_return_2nd_group_with_roster_fixed_titles_set_in_nul = () =>
            GetGroup(g2Id).FixedRosterTitles.ShouldBeNull();

        It should_return_2nd_group_with_roster_scope_ids_contains_q2Id = () =>
            GetGroup(g2Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_3rd_group_with_breadcrumbs_with_g5Id_g2Id = () =>
            GetGroup(g3Id).ParentGroupsIds.ShouldContainOnly(g5Id, g2Id);

        It should_return_3rd_group_with_roster_scope_ids_with_g3Id_q2Id = () =>
            GetGroup(g3Id).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        It should_return_4th_group_with_breadcrumbs_with_g5Id_g2Id = () =>
            GetGroup(g4Id).ParentGroupsIds.ShouldContainOnly(g5Id, g2Id);

        It should_return_4th_group_with_roster_scope_ids_contains_q2Id = () =>
            GetGroup(g4Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_3rd_group_with_parent_id_equals_g2Id = () =>
            GetGroup(g3Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_4th_group_with_parent_id_equals_g2Id = () =>
            GetGroup(g4Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_question_N3_with_parent_id_equals_g2Id = () =>
            GetQuestion(q3Id).ParentGroupId.ShouldEqual(g2Id);

        It should_return_question_N4_with_parent_id_equals_g3Id = () =>
            GetQuestion(q4Id).ParentGroupId.ShouldEqual(g3Id);

        It should_return_question_N5_with_parent_id_equals_g4Id = () =>
            GetQuestion(q5Id).ParentGroupId.ShouldEqual(g4Id);

        It should_return_question_N3_with_breadcrumbs_with_g1Id_g2Id = () =>
            GetQuestion(q3Id).ParentGroupsIds.ShouldContainOnly(g5Id, g2Id);

        It should_return_question_N4_with_breadcrumbs_with_g1Id_g2Id_g3Id = () =>
            GetQuestion(q4Id).ParentGroupsIds.ShouldContainOnly(g5Id, g2Id, g3Id);

        It should_return_question_N5_with_breadcrumbs_with_g1Id_g2Id_g4Id = () =>
            GetQuestion(q5Id).ParentGroupsIds.ShouldContainOnly(g5Id, g2Id, g4Id);

        It should_return_question_N3_with_roster_scope_ids_with_q2Id = () =>
            GetQuestion(q3Id).RosterScopeIds.ShouldContainOnly(q2Id);

        It should_return_question_N4_with_roster_scope_ids_with_g3Id_q2Id = () =>
            GetQuestion(q4Id).RosterScopeIds.ShouldContainOnly(g3Id, q2Id);

        It should_return_question_N5_with_roster_scope_ids_with_q2Id = () =>
            GetQuestion(q5Id).RosterScopeIds.ShouldContainOnly(q2Id);

        private static QuestionDetailsView GetQuestion(Guid questionId)
        {
            return newState.Questions.Single(x => x.Id == questionId);
        }

        private static GroupAndRosterDetailsView GetGroup(Guid groupId)
        {
            return newState.Groups.Single(x => x.Id == groupId);
        }

        private static QuestionsAndGroupsCollectionDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> evnt;
        private static QuestionsAndGroupsCollectionView newState = null;
        private static Mock<IQuestionDetailsViewMapper> questionDetailsFactoryMock = null;
        private static Mock<IQuestionnaireEntityFactory> questionFactoryMock;
    }
}