using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_QuestionnaireItemMoved_event_received_and_group_moved_to_questionnaire : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(viewState);
            denormalizer.Update(viewState, Create.NewGroupAddedEvent(groupId: groupId, parentGroupId: chapterId, groupTitle: groupTitle));
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.QuestionnaireItemMovedEvent(itemId: groupId));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_contains_2_chapters = () =>
            viewState.Chapters.Count.ShouldEqual(2);

        It should_questionnnaireInfoView_first_chapter_id_be_equal_to_groupId = () =>
            viewState.Chapters[0].ItemId.ShouldEqual(groupId);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_to_groupTitle = () =>
            viewState.Chapters[0].Title.ShouldEqual(groupTitle);

        It should_questionnnaireInfoView_second_chapter_id_be_equal_to_chapterId = () =>
            viewState.Chapters[1].ItemId.ShouldEqual(chapterId);

        private static string chapterId = "33333333333333333333333333333333";
        private static string groupId = "22222222222222222222222222222222";
        private static string groupTitle = "group title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
