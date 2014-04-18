using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_chapter_and_chapter_exists : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var chapterInfoView = CreateChapterInfoView();

            var chapterInfoViewFactory =
                Mock.Of<IViewFactory<ChapterInfoViewInputModel, IQuestionnaireItem>>(
                    x => x.Load(Moq.It.IsAny<ChapterInfoViewInputModel>()) == chapterInfoView);

            controller = CreateQuestionnaireController(chapterInfoViewFactory: chapterInfoViewFactory);
        };

        Because of = () =>
            result = controller.Chapter(questionnaireId, chapterId);

        It should_chapter_not_be_null = () =>
            result.ShouldNotBeNull();

        It should_chapter_has_type_of_GroupInfoView = () =>
            result.ShouldBeOfExactType<GroupInfoView>();

        private static QuestionnaireController controller;
        private static IQuestionnaireItem result;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}