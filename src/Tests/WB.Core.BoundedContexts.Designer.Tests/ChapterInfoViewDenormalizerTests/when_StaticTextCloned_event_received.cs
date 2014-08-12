using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_StaticTextCloned_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1ChapterAnd1StaticTextInsideChapter(chapterId, sourceEntityId);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState,
                    Create.StaticTextClonedEvent(entityId: entityId, parentId: chapterId,
                        text: text, sourceEntityId: sourceEntityId, targetIndex: targetIndex));

        It should_groupInfoView_first_chapter_items_not_be_null = () =>
            ((GroupInfoView)viewState.Items[0]).Items.ShouldNotBeNull();

        It should_groupInfoView_first_chapter_items_count_be_equal_to_2 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(2);

        It should_groupInfoView_first_chapter_first_item_type_be_equal_to_StaticTextInfoView = () =>
            ((GroupInfoView)viewState.Items[0]).Items[targetIndex].ShouldBeOfExactType<StaticTextInfoView>();

        It should_groupInfoView_first_chapter_first_item_id_be_equal_to_entityId = () =>
            ((GroupInfoView)viewState.Items[0]).Items[targetIndex].ItemId.ShouldEqual(entityId);

        It should_groupInfoView_static_text_be_equal_to_specified_text = () =>
            ((StaticTextInfoView)((GroupInfoView)viewState.Items[targetIndex]).Items[0]).Text.ShouldEqual(text);

        private static string chapterId = "33333333333333333333333333333333";
        private static string entityId = "22222222222222222222222222222222";
        private static string sourceEntityId = "11111111111111111111111111111111";
        private static string text = "some text";
        private static int targetIndex = 0;

        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
