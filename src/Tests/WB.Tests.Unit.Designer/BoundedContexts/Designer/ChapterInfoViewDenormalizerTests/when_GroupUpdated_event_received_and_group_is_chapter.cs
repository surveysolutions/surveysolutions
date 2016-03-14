using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_GroupUpdated_event_received_and_group_is_chapter : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupUpdatedEvent(groupId: chapterId, groupTitle: chapterTitle));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Items.ShouldNotBeNull();

        It should_questionnnaireInfoView_first_chapter_title_be_equal_to_chapterTitle = () =>
            ((GroupInfoView)viewState.Items[0]).Title.ShouldEqual(chapterTitle);

        private static string chapterId = "33333333333333333333333333333333";
        private static string chapterTitle = "chapter title";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
