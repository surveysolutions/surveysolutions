using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewDenormalizerTests
{
    internal class when_QuestionnaireItemMoved_event_received_and_question_moved_from_the_first_position_to_the_last_one : ChaptersInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateGroupInfoViewWith1ChapterAnd3QuestionInsideChapter(chapter1Id, question1Id, question2Id, question3Id);
            denormalizer = CreateDenormalizer(view: viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.QuestionnaireItemMovedEvent(itemId: question1Id, targetGroupId: chapter1Id, targetIndex:3));
        
        It should_groupInfoView_items_count_be_equal_to_3 = () =>
            ((GroupInfoView)viewState.Items[0]).Items.Count.ShouldEqual(3);
        
        It should_groupInfoView_first_item_id_be_equal_to_question2Id = () =>
            ((GroupInfoView)viewState.Items[0]).Items[0].ItemId.ShouldEqual(question2Id);

        It should_groupInfoView_last_item_id_be_equal_to_question1Id = () =>
            ((GroupInfoView)viewState.Items[0]).Items[2].ItemId.ShouldEqual(question1Id);

        private static string chapter1Id = "33333333333333333333333333333333";
        private static string question1Id = "11111111111111111111111111111111";
        private static string question2Id = "22222222222222222222222222222222";
        private static string question3Id = "44444444444444444444444444444444";

        private static ChaptersInfoViewDenormalizer denormalizer;
        private static GroupInfoView viewState;
    }
}