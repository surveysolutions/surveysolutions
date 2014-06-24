using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.ChapterInfoViewDenormalizerTests
{
    internal class when_GroupStoppedBeingARoster_event_received : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1ChapterAnd1RosterInsideChapter(chapterId: chapterId, rosterId: groupId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupStoppedBeingARosterEvent(groupId: groupId));

        It should_questionnnaireInfoView_has_group_with_isroster_property_equal_to_false = () =>
            GetFirstGroupInFirstChapter().IsRoster.ShouldEqual(false);

        private static GroupInfoView GetFirstGroupInFirstChapter()
        {
            return (GroupInfoView)((GroupInfoView)viewState.Items[0]).Items[0];
        }

        private static string chapterId = "33333333333333333333333333333333";
        private static string groupId = "22222222222222222222222222222222";
        
        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}
