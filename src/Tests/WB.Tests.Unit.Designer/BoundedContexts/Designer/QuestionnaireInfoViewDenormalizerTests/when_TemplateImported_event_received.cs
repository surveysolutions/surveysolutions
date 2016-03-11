using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_TemplateImported_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState =
                denormalizer.Update(null, Create.TemplateImportedEvent(questionnaireId: questionnaireId,
                    questionnaireTitle: questionnaireTitle, chapter1Id: chapter1Id, chapter1Title: chapter1Title,
                    chapter2Id: chapter2Id, chapter2Title: chapter2Title,
                    isPublic: true));

        It should_questionnnaireInfoView_QuestionnaireId_be_equal_to_questionnaireId = () =>
            viewState.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_have_2_chapters = () =>
            viewState.Chapters.Count.ShouldEqual(2);

        It should_questionnnaireInfoView_first_chapter_id_be_equal_chapter1Id = () =>
            viewState.Chapters[0].ItemId.ShouldEqual(chapter1Id);

        It should_questionnnaireInfoView_second_chapter_id_be_equal_chapter2Id = () =>
            viewState.Chapters[1].ItemId.ShouldEqual(chapter2Id);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_chapter1Title = () =>
            viewState.Chapters[0].Title.ShouldEqual(chapter1Title);

        It should_questionnnaireInfoView_second_chapter_title_be_equal_chapter2Title = () =>
            viewState.Chapters[1].Title.ShouldEqual(chapter2Title);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        private static string chapter1Id = "22222222222222222222222222222222";
        private static string chapter2Id = "11111111111111111111111111111111";
        private static string chapter1Title = "chapter 1 title";
        private static string chapter2Title = "chapter 2 title";

        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
