using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewFactoryTests
{
    internal class when_loading_view_and_chapter_exists : ChapterInfoViewFactoryContext
    {
        Establish context = () =>
        {
            input = CreateChapterInfoViewInputModel(questionnaireId, chapterId);

            var repositoryMock = new Mock<IQueryableReadSideRepositoryReader<GroupInfoView>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(CreateChapterInfoView(questionnaireId, chapterId));

            factory = CreateChapterInfoViewFactory(repository: repositoryMock.Object);
        };

        Because of = () =>
            view = factory.Load(input);

        It should_find_chapter = () =>
            view.ShouldNotBeNull();

        It should_chapter_id_be_equal_chapterId = () =>
            view.Id.ShouldEqual(chapterId);

        private static IQuestionnaireItem view;
        private static ChapterInfoViewInputModel input;
        private static ChapterInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}
