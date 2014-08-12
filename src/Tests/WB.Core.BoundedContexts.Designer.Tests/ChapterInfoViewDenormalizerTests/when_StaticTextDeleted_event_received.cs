using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_StaticTextDeleted_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1ChapterAnd1StaticTextInsideChapter(chapterId, staticTextId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState = denormalizer.Update(viewState, Create.StaticTextDeletedEvent(staticTextId));

        It should_groupInfoView_first_chapter_items_be_empty = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldBeEmpty();

        private static string chapterId = "33333333333333333333333333333333";
        private static string staticTextId = "22222222222222222222222222222222";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
