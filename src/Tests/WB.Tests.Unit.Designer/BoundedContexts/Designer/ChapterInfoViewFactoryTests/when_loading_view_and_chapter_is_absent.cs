using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class when_loading_view_and_chapter_is_absent : ChapterInfoViewFactoryContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var repositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(CreateQuestionnaireDocumentWithoutChapters(questionnaireId));

            factory = CreateChapterInfoViewFactory(repository: repositoryMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            view = factory.Load(questionnaireId, chapterId);

        [NUnit.Framework.Test] public void should_chapter_be_null () =>
            view.ShouldBeNull();

        private static NewChapterView view;
        private static ChapterInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}
