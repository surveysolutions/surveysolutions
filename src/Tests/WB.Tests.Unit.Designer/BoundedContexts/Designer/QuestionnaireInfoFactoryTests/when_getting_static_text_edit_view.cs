using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionnaireEntityDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            questionnaireView = CreateQuestionsAndGroupsCollectionView();
            questionnaireEntityDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetStaticTextEditView(questionnaireId, entityId);

        It should_return_not_null_view = () =>
            result.ShouldNotBeNull();

        It should_return_question_with_Id_equals_questionId = () =>
            result.Id.ShouldEqual(entityId);

        It should_return_question_equals_g3 = () =>
            result.Text.ShouldEqual(GetStaticText(entityId).Text);

        private static StaticTextDetailsView GetStaticText(Guid entityId)
        {
            return questionnaireView.StaticTexts.Single(x => x.Id == entityId);
        }

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = st1Id;

    }
}