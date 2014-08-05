using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_GroupCloned_event_received_and_new_group_is_chapter : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(chapter1Id);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupClonedEvent(groupId: chapter2Id, groupTitle: chapter2Title));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_contains_2_chapters = () =>
            viewState.Chapters.Count.ShouldEqual(2);

        It should_questionnnaireInfoView_first_chapter_id_be_equal_to_chapter2Id = () =>
            viewState.Chapters[0].ItemId.ShouldEqual(chapter2Id);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_to_chapterTitle = () =>
            viewState.Chapters[0].Title.ShouldEqual(chapter2Title);

        private static string chapter1Id = "33333333333333333333333333333333";
        private static string chapter2Id = "22222222222222222222222222222222";
        private static string chapter2Title = "chapter title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
