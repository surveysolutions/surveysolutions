using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_QuestionnaireItemMoved_event_received_and_group_moved_from_one_chapter_to_another_chapter : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith2ChaptersAndGroupsInThem(chapter1Id, chapter2Id, group1Id, group2Id);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.QuestionnaireItemMovedEvent(itemId: group1Id, targetGroupId: chapter2Id));

        It should_groupInfoView_first_chapter_items_be_empty = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldBeEmpty();

        It should_groupInfoView_second_chapter_items_count_be_equal_to_2 = () =>
            ((GroupInfoView)viewState.Items[1]).Items.Count.ShouldEqual(2);

        It should_groupInfoView_second_chapter_first_item_type_be_equal_to_GroupInfoView = () =>
            ((GroupInfoView)viewState.Items[1]).Items[0].ShouldBeOfExactType<GroupInfoView>();

        It should_groupInfoView_second_chapter_first_item_id_be_equal_to_group1Id = () =>
            ((GroupInfoView)viewState.Items[1]).Items[0].ItemId.ShouldEqual(group1Id);

        private static string chapter1Id = "33333333333333333333333333333333";
        private static string chapter2Id = "11111111111111111111111111111111";
        private static string group1Id = "22222222222222222222222222222222";
        private static string group2Id = "44444444444444444444444444444444";

        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}