using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_fixed_roster_edit_view : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = CreateQuestionnaireDocument();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        [NUnit.Framework.Test] public void should_return_not_null_view () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_ItemId_equals_groupId () =>
            result.ItemId.Should().Be(rosterId.FormatGuid());

        [NUnit.Framework.Test] public void should_return_roster_with_Title_equals_g3_title () =>
            result.Title.Should().Be(GetGroup(rosterId).Title);

        [NUnit.Framework.Test] public void should_return_roster_with_EnablementCondition_equals_g3_enablementCondition () =>
            result.EnablementCondition.Should().Be(GetGroup(rosterId).ConditionExpression);

        [NUnit.Framework.Test] public void should_return_roster_with_RosterFixedTitles_equals_g3_RosterFixedTitles () =>
            result.FixedRosterTitles.Should().Contain(GetGroup(rosterId).FixedRosterTitles);

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeMultiQuestionId_be_null () =>
            result.RosterSizeMultiQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeListQuestionId_be_null () =>
            result.RosterSizeListQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeNumericQuestionId_be_null () =>
            result.RosterSizeNumericQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_Type_equals_to_Fixed () =>
            result.Type.Should().Be(RosterType.Fixed);

        [NUnit.Framework.Test] public void should_return_roster_with_RosterTitleQuestionId_equals_g3_RosterTitleQuestionId () =>
            result.RosterTitleQuestionId.Should().Be(GetGroup(rosterId).RosterTitleQuestionId.FormatGuid());

        [NUnit.Framework.Test] public void should_return_grouped_list_of_multi_questions_with_one_pair () =>
            result.NotLinkedMultiOptionQuestions.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_grouped_list_of_multi_questions_with_one_pair_and_key_equals_ () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(0).Title.Should().Be("Group 1");

        [NUnit.Framework.Test] public void should_return_grouped_list_of_multi_questions_with_values_ids_contains_only_q2Id () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(1).Id.Should().Contain(q2Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_grouped_list_of_multi_questions_with_values_titles_contains_only_q2Id () =>
            result.NotLinkedMultiOptionQuestions.ElementAt(1).Title.Should().Contain(GetQuestion(q2Id).QuestionText);

        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_titles_with_one_pair () =>
            result.NumericIntegerTitles.Count.Should().Be(3);

        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_titles_with_two_pairs_and_key_equals__textListGroupKey () =>
            result.NumericIntegerTitles.ElementAt(0).Title.Should().Be(textListGroupKey);

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q4Id () =>
            result.NumericIntegerTitles.ElementAt(1).Id.Should().Contain(q4Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_index_2_with_ids_contains_only_q7Id () =>
            result.NumericIntegerTitles.ElementAt(2).Id.Should().Contain(q7Id.FormatGuid());

        [NUnit.Framework.Test] public void should_list_of_roster_title_do_not_countain_multiomedia_question_with_id_q8Id () =>
            result.NumericIntegerTitles.Should().OnlyContain(q => q.Id != q8Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_questions_with_two_pair () =>
            result.NumericIntegerQuestions.Count.Should().Be(4);

        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_questions_with_two_pairs_and_key_equals__group_1__group_2 () =>
            result.NumericIntegerQuestions.Where(x => x.IsSectionPlaceHolder).Select(x => x.Title).Should().Contain("Group 1", "Group 2");

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_key__Group_1__with_ids_contains_only_q1Id () =>
            result.NumericIntegerQuestions.ElementAt(1).Id.Should().Be(numericQuestionId.FormatGuid());

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_key__Group_1__with_titles_contains_only_q1_title () =>
            result.NumericIntegerQuestions.ElementAt(1).Title.Should().Be(GetQuestion(numericQuestionId).QuestionText);

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_key__Group_2__with_ids_contains_only_q1Id () =>
            result.NumericIntegerQuestions.ElementAt(3).Id.Should().Be(q6Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_integer_questions_in_group_with_key__Group_2__with_titles_contains_only_q1_title () =>
            result.NumericIntegerQuestions.ElementAt(3).Title.Should().Be(GetQuestion(q6Id).QuestionText);

        [NUnit.Framework.Test] public void should_return_grouped_list_of_integer_questions_with_one_pair () =>
            result.TextListsQuestions.Count.Should().Be(0);

        private static IGroup GetGroup(Guid groupId)
        {
            return questionnaireView.Find<IGroup>(groupId);
        }

        private static IQuestion GetQuestion(Guid questionId)
        {
            return questionnaireView.Find<IQuestion>(questionId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid rosterId = g3Id;
        private static string textListGroupKey = "Group 1 / Roster 1.1 / Roster 1.1.1";
    }
}
