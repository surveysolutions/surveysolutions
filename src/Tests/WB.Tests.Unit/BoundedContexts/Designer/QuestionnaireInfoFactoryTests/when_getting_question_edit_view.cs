using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_question_edit_view : QuestionnaireInfoFactoryTestContext
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
            result = factory.GetQuestionEditView(questionnaireId, questionId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_question_with_Id_equals_questionId = () =>
            result.Id.ShouldEqual(questionId);

        It should_return_question_equals_g3 = () =>
            result.Title.ShouldEqual(GetQuestion(questionId).Title);

        It should_return_grouped_list_of_multi_questions_with_one_pair = () =>
            result.SourceOfLinkedQuestions.Count.ShouldEqual(6);

        It should_replace_guids_in_condition_expressions_for_var_names = () =>
            result.EnablementCondition.ShouldEqual("[q1] > 25");

        It should_return_grouped_list_of_multi_questions_with_one_pair_and_key_equals_ = () =>
            result.SourceOfLinkedQuestions.Select(x => x.Title).ShouldContain(linkedQuestionsKey1, linkedQuestionsKey2);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey1__with_ids_contains_only_q3Id = () =>
            result.SourceOfLinkedQuestions.Select(x => x.Id).ShouldContain(q3Id.FormatGuid());

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey1__with_titles_contains_only_q3_title = () =>
            result.SourceOfLinkedQuestions.Select(x => x.Title).ShouldContain(GetQuestion(q3Id).Title);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey2__with_ids_contains_only_q5Id = () =>
            result.SourceOfLinkedQuestions.Select(x => x.Id).ShouldContain(q5Id.FormatGuid());

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey2__with_titles_contains_only_q5_title = () =>
            result.SourceOfLinkedQuestions.Select(x => x.Title).ShouldContain(GetQuestion(q5Id).Title);

        private static QuestionDetailsView GetQuestion(Guid questionId)
        {
            return questionnaireView.Questions.Single(x => x.Id == questionId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditQuestionView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid questionId = q2Id;
        private static string linkedQuestionsKey1 = "Group 1 / Roster 1.1";
        private static string linkedQuestionsKey2 = "Group 1 / Roster 1.1 / Group 1.1.2";

    }
}