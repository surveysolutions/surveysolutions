using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_NewGroupAdded_event_received_and_new_group_is_chapter : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoView();
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.NewGroupAddedEvent(groupId: chapterId, groupTitle: chapterTitle));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_contains_1_chapter = () =>
            viewState.Chapters.Count.ShouldEqual(1);

        It should_questionnnaireInfoView_first_chapter_id_be_equal_to_chapterId = () =>
            viewState.Chapters[0].ItemId.ShouldEqual(chapterId);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_to_chapterTitle = () =>
            viewState.Chapters[0].Title.ShouldEqual(chapterTitle);

        private static string chapterId = "33333333333333333333333333333333";
        private static string chapterTitle = "chapter title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
