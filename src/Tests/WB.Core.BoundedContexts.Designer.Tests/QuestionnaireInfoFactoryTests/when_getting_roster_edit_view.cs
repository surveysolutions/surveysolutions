using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoFactoryTests
{
    internal class when_getting_roster_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionView();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_group_with_Id_equals_groupId = () =>
            result.Roster.Id.ShouldEqual(rosterId);

        It should_return_group_with_Title_equals_g3_title = () =>
            result.Roster.Title.ShouldEqual(GetGroup(rosterId).Title);

        It should_return_group_with_EnablementCondition_equals_g3_enablementCondition = () =>
            result.Roster.EnablementCondition.ShouldEqual(GetGroup(rosterId).EnablementCondition);

        It should_return_group_with_Description_equals_g3_description = () =>
            result.Roster.Description.ShouldEqual(GetGroup(rosterId).Description);

        It should_return_group_with_RosterFixedTitles_equals_g3_RosterFixedTitles = () =>
            result.Roster.RosterFixedTitles.ShouldEqual(GetGroup(rosterId).RosterFixedTitles);

        It should_return_group_with_RosterSizeQuestionId_equals_g3_RosterSizeQuestionId = () =>
            result.Roster.RosterSizeQuestionId.ShouldEqual(GetGroup(rosterId).RosterSizeQuestionId);

        It should_return_group_with_RosterSizeSourceType_equals_g3_RosterSizeSourceType = () =>
            result.Roster.RosterSizeSourceType.ShouldEqual(GetGroup(rosterId).RosterSizeSourceType);

        It should_return_group_with_RosterTitleQuestionId_equals_g3_RosterTitleQuestionId = () =>
            result.Roster.RosterTitleQuestionId.ShouldEqual(GetGroup(rosterId).RosterTitleQuestionId);

        It should_return_grouped_list_of_multi_questions_with_one_pair = () =>
            result.NotLinkedMultiOptionQuestions.Count.ShouldEqual(1);

        It should_return_grouped_list_of_multi_questions_with_one_pair_and_key_equals_ = () =>
            result.NotLinkedMultiOptionQuestions.Keys.ShouldContainOnly("Group 1");

        It should_return_grouped_list_of_multi_questions_with_values_ids_contains_only_q2Id = () =>
            result.NotLinkedMultiOptionQuestions["Group 1"].Select(x => x.Id).ShouldContainOnly(q2Id);

        It should_return_grouped_list_of_multi_questions_with_values_titles_contains_only_q2Id = () =>
            result.NotLinkedMultiOptionQuestions["Group 1"].Select(x => x.Title).ShouldContainOnly(GetQuestion(q2Id).Title);

        It should_return_grouped_list_of_integer_titles_with_one_pair = () =>
            result.NumericIntegerTitles.Count.ShouldEqual(1);

        It should_return_grouped_list_of_integer_titles_with_two_pairs_and_key_equals__textListGroupKey = () =>
            result.NumericIntegerTitles.Keys.ShouldContainOnly(textListGroupKey);

        It should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q4Id = () =>
            result.NumericIntegerTitles[textListGroupKey].Select(x => x.Id).ShouldContainOnly(q4Id);

        It should_return_grouped_list_of_integer_questions_with_two_pair = () =>
            result.NumericIntegerQuestions.Count.ShouldEqual(2);

        It should_return_grouped_list_of_integer_questions_with_two_pairs_and_key_equals__group_1__group_2 = () =>
            result.NumericIntegerQuestions.Keys.ShouldContainOnly("Group 1", "Group 2");

        It should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q1Id = () =>
            result.NumericIntegerQuestions["Group 1"].Select(x => x.Id).ShouldContainOnly(q1Id);

        It should_return_integer_questions_in_group_with_key__Group_1__with_titles_contains_only_q1_title = () =>
            result.NumericIntegerQuestions["Group 1"].Select(x => x.Title).ShouldContainOnly(GetQuestion(q1Id).Title);

        It should_return_integer_questions_in_group_with_key__Group_2__with_ids_contains_only_q1Id = () =>
            result.NumericIntegerQuestions["Group 2"].Select(x => x.Id).ShouldContainOnly(q6Id);

        It should_return_integer_questions_in_group_with_key__Group_2__with_titles_contains_only_q1_title = () =>
            result.NumericIntegerQuestions["Group 2"].Select(x => x.Title).ShouldContainOnly(GetQuestion(q6Id).Title);

        It should_return_grouped_list_of_integer_questions_with_one_pair = () =>
            result.TextListsQuestions.Count.ShouldEqual(1);

        It should_return_grouped_list_of_integer_questions_with_two_pairs_and_key_equals__group_1__group_12__ = () =>
            result.TextListsQuestions.Keys.ShouldContainOnly(textListGroupKey);

        It should_return_integer_questions_in_group_with_key__textListGroupKey__with_ids_contains_only_q4Id = () =>
            result.TextListsQuestions[textListGroupKey].Select(x => x.Id).ShouldContainOnly(q4Id);

        It should_return_integer_questions_in_group_with_key__textListGroupKey__with_titles_contains_only_q4_title = () =>
            result.TextListsQuestions[textListGroupKey].Select(x => x.Title).ShouldContainOnly(GetQuestion(q4Id).Title);
        
        private static GroupAndRosterDetailsView GetGroup(Guid groupId)
        {
            return questionnaireView.Groups.Single(x => x.Id == groupId);
        }

        private static QuestionDetailsView GetQuestion(Guid questionId)
        {
            return questionnaireView.Questions.Single(x => x.Id == questionId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid rosterId = g3Id;
        private static string textListGroupKey = "Group 1 / Roster 1.1 / Roster 1.1.1";
    }
}