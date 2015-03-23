using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_edit_view_and_static_text_is_absent : QuestionnaireInfoFactoryTestContext
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
            result = factory.GetStaticTextEditView(questionnaireId, notExistingEntityId);

        It should_return_null = () =>
            result.ShouldBeNull();

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static QuestionsAndGroupsCollectionView questionnaireView;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid notExistingEntityId = Guid.Parse("22222222222222222222222222222222");
    }
}