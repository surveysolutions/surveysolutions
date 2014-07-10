using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_StaticTextAdded_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState,
                    Create.StaticTextAddedEvent(entityId: entityId, parentId: chapterId, text: text));

        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_items_count_be_equal_to_1 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(1);

        It should_groupInfoView_first_chapter_first_item_type_be_equal_to_StaticTextInfoView = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ShouldBeOfExactType<StaticTextInfoView>();

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_entityId = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ItemId.ShouldEqual(entityId);

        It should_groupInfoView_first_chapter_first_static_text_be_equal_to_specified_text = () =>
            ((StaticTextInfoView)((GroupInfoView)viewState.Items[0]).Items[0]).Text.ShouldEqual(text);

        private static string chapterId = "33333333333333333333333333333333";
        private static string entityId =   "22222222222222222222222222222222";
        private static string text = "some text";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
