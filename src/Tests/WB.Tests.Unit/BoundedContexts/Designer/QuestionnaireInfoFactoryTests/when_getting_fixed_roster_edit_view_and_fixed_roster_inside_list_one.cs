using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_fixed_roster_edit_view_and_fixed_roster_inside_list_one : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionViewWithListQuestions();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        It should_return_empty_grouped_list_of_multi_questions = () =>
            result.NotLinkedMultiOptionQuestions.Count.ShouldEqual(0);

        It should_return_empty_grouped_list_of_integer_questions = () =>
            result.NumericIntegerQuestions.Count.ShouldEqual(0);

        It should_return_empty_grouped_list_of_title_questions = () =>
            result.NumericIntegerTitles.Count.ShouldEqual(0);
       
        It should_return_grouped_list_of_integer_questions_with_one_pair = () =>
            result.TextListsQuestions.Count.ShouldEqual(4);

        It should_return_list_questions_at_1_with_id_equals_q1Id = () =>
            result.TextListsQuestions.ElementAt(1).Id.ShouldContainOnly(q1Id.FormatGuid());

        It should_return_list_questions_at_1_with_q1_title = () =>
            result.TextListsQuestions.ElementAt(1).Title.ShouldContainOnly(GetQuestion(q1Id).Title);

        It should_return_list_questions_at_3_with_id_equals_q2Id = () =>
            result.TextListsQuestions.ElementAt(3).Id.ShouldContainOnly(q2Id.FormatGuid());

        It should_return_list_questions_at_3_with_q2_title = () =>
            result.TextListsQuestions.ElementAt(3).Title.ShouldContainOnly(GetQuestion(q2Id).Title);

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
    }
}