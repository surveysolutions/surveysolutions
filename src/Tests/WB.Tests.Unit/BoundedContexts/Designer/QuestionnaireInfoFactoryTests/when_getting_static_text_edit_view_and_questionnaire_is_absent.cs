using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_static_text_edit_view_and_questionnaire_is_absent : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionnaireEntityDetailsReaderMock = new Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>();
            factory = CreateQuestionnaireInfoFactory(questionnaireEntityDetailsReaderMock.Object);
        };

        Because of = () =>
            result = factory.GetStaticTextEditView(questionnaireId, entityId);

        It should_return_null = () =>
            result.ShouldBeNull();

        private static QuestionnaireInfoFactory factory;
        private static NewEditStaticTextView result;
        private static Mock<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>> questionnaireEntityDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid entityId = st1Id;
    }
}