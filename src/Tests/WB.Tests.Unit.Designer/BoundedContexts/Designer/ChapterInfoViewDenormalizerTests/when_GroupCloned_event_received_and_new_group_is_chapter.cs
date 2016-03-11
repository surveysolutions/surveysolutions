using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_GroupCloned_event_received_and_new_group_is_chapter : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1Chapter(chapter1Id);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupClonedEvent(groupId: chapter2Id, groupTitle: chapter2Title));

        It should_groupInfoView_Items_not_be_null = () =>
            viewState.Items.ShouldNotBeNull();

        It should_groupInfoView_Items_contains_2_chapters = () =>
            viewState.Items.Count.ShouldEqual(2);

        It should_groupInfoView_first_item_id_be_equal_to_chapter2Id = () =>
            viewState.Items[0].ItemId.ShouldEqual(chapter2Id);

        It should_groupInfoView_first_item_title_be_equal_to_chapterTitle = () =>
            ((GroupInfoView)viewState.Items[0]).Title.ShouldEqual(chapter2Title);

        private static string chapter1Id = "33333333333333333333333333333333";
        private static string chapter2Id = "22222222222222222222222222222222";
        private static string chapter2Title = "chapter title";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
