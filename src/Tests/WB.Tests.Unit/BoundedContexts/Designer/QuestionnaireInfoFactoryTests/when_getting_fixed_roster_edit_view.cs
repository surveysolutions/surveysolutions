using System;
using System.Linq;
using System.Web.UI.WebControls;
using FluentAssertions;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_fixed_roster_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
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

        It should_return_roster_with_ItemId_equals_groupId = () =>
            result.ItemId.ShouldEqual(rosterId.FormatGuid());

        It should_return_roster_with_Title_equals_g3_title = () =>
            result.Title.ShouldEqual(GetGroup(rosterId).Title);

        It should_return_roster_with_EnablementCondition_equals_g3_enablementCondition = () =>
            result.EnablementCondition.ShouldEqual(GetGroup(rosterId).EnablementCondition);

        It should_return_roster_with_RosterFixedTitles_equals_g3_RosterFixedTitles = () =>
            result.FixedRosterTitles.ShouldContainOnly(GetGroup(rosterId).FixedRosterTitles);

        It should_return_roster_with_RosterSizeMultiQuestionId_be_null = () =>
            result.RosterSizeMultiQuestionId.ShouldBeNull();

        It should_return_roster_with_RosterSizeListQuestionId_be_null = () =>
            result.RosterSizeListQuestionId.ShouldBeNull();

        It should_return_roster_with_RosterSizeNumericQuestionId_be_null = () =>
            result.RosterSizeNumericQuestionId.ShouldBeNull();

        It should_return_roster_with_Type_equals_to_Fixed = () =>
            result.Type.ShouldEqual(RosterType.Fixed);

        It should_return_roster_with_RosterTitleQuestionId_equals_g3_RosterTitleQuestionId = () =>
            result.RosterTitleQuestionId.ShouldEqual(GetGroup(rosterId).RosterTitleQuestionId.FormatGuid());

        It should_return_grouped_list_of_multi_questions_with_one_pair = () =>
            result.NotLinkedMultiOptionQuestions.Count.ShouldEqual(2);

        It should_return_grouped_list_of_multi_questions_with_one_pair_and_key_equals_ = () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(0).Title.ShouldEqual("Group 1");

        It should_return_grouped_list_of_multi_questions_with_values_ids_contains_only_q2Id = () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(1).Id.ShouldContainOnly(q2Id.FormatGuid());

        It should_return_grouped_list_of_multi_questions_with_values_titles_contains_only_q2Id = () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(1).Title.ShouldContainOnly(GetQuestion(q2Id).Title);

        It should_return_grouped_list_of_integer_titles_with_one_pair = () =>
            result.NumericIntegerTitles.Count.ShouldEqual(3);

        It should_return_grouped_list_of_integer_titles_with_two_pairs_and_key_equals__textListGroupKey = () =>
            result.NumericIntegerTitles.ElementAt(0).Title.ShouldEqual(textListGroupKey);

        It should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q4Id = () =>
            result.NumericIntegerTitles.ElementAt(1).Id.ShouldContainOnly(q4Id.FormatGuid());

        It should_return_integer_questions_in_group_with_index_2_with_ids_contains_only_q7Id = () =>
            result.NumericIntegerTitles.ElementAt(2).Id.ShouldContainOnly(q7Id.FormatGuid());

        It should_list_of_roster_title_do_not_countain_multiomedia_question_with_id_q8Id = () =>
            result.NumericIntegerTitles.ShouldEachConformTo(q => q.Id != q8Id.FormatGuid());

        It should_return_grouped_list_of_integer_questions_with_two_pair = () =>
            result.NumericIntegerQuestions.Count.ShouldEqual(4);

        It should_return_grouped_list_of_integer_questions_with_two_pairs_and_key_equals__group_1__group_2 = () =>
            result.NumericIntegerQuestions.Where(x => x.IsSectionPlaceHolder).Select(x => x.Title).ShouldContainOnly("Group 1", "Group 2");

        It should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q1Id = () =>
            result.NumericIntegerQuestions.ElementAt(1).Id.ShouldContainOnly(q1Id.FormatGuid());

        It should_return_integer_questions_in_group_with_key__Group_1__with_titles_contains_only_q1_title = () =>
            result.NumericIntegerQuestions.ElementAt(1).Title.ShouldContainOnly(GetQuestion(q1Id).Title);

        It should_return_integer_questions_in_group_with_key__Group_2__with_ids_contains_only_q1Id = () =>
            result.NumericIntegerQuestions.ElementAt(3).Id.ShouldContainOnly(q6Id.FormatGuid());

        It should_return_integer_questions_in_group_with_key__Group_2__with_titles_contains_only_q1_title = () =>
            result.NumericIntegerQuestions.ElementAt(3).Title.ShouldContainOnly(GetQuestion(q6Id).Title);

        It should_return_grouped_list_of_integer_questions_with_one_pair = () =>
            result.TextListsQuestions.Count.ShouldEqual(0);

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
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid rosterId = g3Id;
        private static string textListGroupKey = "Group 1 / Roster 1.1 / Roster 1.1.1";
    }
}