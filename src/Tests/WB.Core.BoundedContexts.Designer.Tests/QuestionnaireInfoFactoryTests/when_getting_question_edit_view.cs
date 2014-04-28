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
    internal class when_getting_question_edit_view : QuestionnaireInfoFactoryTestContext
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
            result = factory.GetQuestionEditView(questionnaireId, questionId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_question_with_Id_equals_questionId = () =>
            result.Question.Id.ShouldEqual(questionId);

        It should_return_question_equals_g3 = () =>
            result.Question.ShouldEqual(GetQuestion(questionId));

        It should_return_grouped_list_of_multi_questions_with_one_pair = () =>
            result.SourceOfLinkedQuestions.Count.ShouldEqual(2);

        It should_return_grouped_list_of_multi_questions_with_one_pair_and_key_equals_ = () =>
            result.SourceOfLinkedQuestions.Keys.ShouldContainOnly(linkedQuestionsKey1, linkedQuestionsKey2);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey1__with_ids_contains_only_q3Id = () =>
            result.SourceOfLinkedQuestions[linkedQuestionsKey1].Select(x => x.Id).ShouldContainOnly(q3Id);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey1__with_titles_contains_only_q3_title = () =>
            result.SourceOfLinkedQuestions[linkedQuestionsKey1].Select(x => x.Title).ShouldContainOnly(GetQuestion(q3Id).Title);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey2__with_ids_contains_only_q5Id = () =>
            result.SourceOfLinkedQuestions[linkedQuestionsKey2].Select(x => x.Id).ShouldContainOnly(q5Id);

        It should_return_integer_questions_in_group_with_key__linkedQuestionsKey2__with_titles_contains_only_q5_title = () =>
            result.SourceOfLinkedQuestions[linkedQuestionsKey2].Select(x => x.Title).ShouldContainOnly(GetQuestion(q5Id).Title);

        private static QuestionDetailsView GetQuestion(Guid questionId)
        {
            return questionnaireView.Questions.Single(x => x.Id == questionId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditQuestionView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideRepositoryReader<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid questionId = q3Id;
        private static string linkedQuestionsKey1 = "Group 1 / Roster 1.1";
        private static string linkedQuestionsKey2 = "Group 1 / Roster 1.1 / Group 1.1.2";

    }
}