using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_NewGroupAdded_event_received_and_new_group_inside_chapter : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.NewGroupAddedEvent(groupId: groupId, parentGroupId: chapterId, groupTitle: groupTitle));

        It should_groupInfoView_Items_not_be_null = () =>
            viewState.Items.ShouldNotBeNull();

        It should_groupInfoView_Items_contains_1_chapter = () =>
            viewState.Items.Count.ShouldEqual(1);

        It should_groupInfoView_first_item_id_be_equal_to_chapterId = () =>
            viewState.Items[0].ItemId.ShouldEqual(chapterId);

        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_items_count_be_equal_to_1 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(1);

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_groupId = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ItemId.ShouldEqual(groupId);

        It should_groupInfoView_first_chapter_first_item_title_be_equal_to_groupTitle = () =>
            ((GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Title.ShouldEqual(groupTitle);

        private static string chapterId = "33333333333333333333333333333333";
        private static string groupId =   "22222222222222222222222222222222";
        private static string groupTitle = "chapter 1 group 1 title";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
