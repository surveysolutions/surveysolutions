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
    internal class when_getting_cascading_question_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionViewWithCascadingQuestions();
            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetQuestionEditView(questionnaireId, questionId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_grouped_list_of_single_questions_with_3_items = () =>
            result.SourceOfSingleQuestions.Count.ShouldEqual(3);

        It should_return_list_withfirst_placeholder_item = () =>
            result.SourceOfSingleQuestions.ElementAt(0).IsSectionPlaceHolder.ShouldBeTrue();

        It should_return_single_question_with_id__g1 = () =>
            result.SourceOfSingleQuestions.ElementAt(1).Id.ShouldEqual(q1Id.FormatGuid());

        It should_return_single_question_with_id__g2 = () =>
            result.SourceOfSingleQuestions.ElementAt(2).Id.ShouldEqual(q2Id.FormatGuid());

        private static QuestionnaireInfoFactory factory;
        private static NewEditQuestionView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid questionId = q3Id;
    }
}