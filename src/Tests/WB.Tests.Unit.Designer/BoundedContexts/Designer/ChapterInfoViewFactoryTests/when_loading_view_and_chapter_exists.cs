using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class when_loading_view_and_chapter_exists : ChapterInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var repositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(CreateQuestionnaireDocument(questionnaireId, chapterId));

            factory = CreateChapterInfoViewFactory(repository: repositoryMock.Object);
        };

        Because of = () =>
            view = factory.Load(questionnaireId, chapterId);

        It should_find_chapter = () =>
            view.ShouldNotBeNull();

        It should_chapter_id_be_equal_chapterId = () =>
            view.Chapter.ItemId.ShouldEqual(chapterId);

        It should_view_have_all_variabe_names_ = () =>
            view.VariableNames.Length.ShouldEqual(keywords.Length);

        It should_contain_all_variabe_names_ = () =>
            view.VariableNames.Select(x => x.Name).ShouldContain(keywords);

        private static NewChapterView view;
        private static ChapterInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
        private static string[] keywords =
        {
            "self",
            "@optioncode",
            "@rowindex",
            "@rowname",
            "@rowcode"
        };
}
}
