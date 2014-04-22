using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_QuestionnaireItemMoved_event_received_and_chapter_moved_to_another_chapter : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith2Chapters(chapter1Id, chapter2Id);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState,
                    CreatePublishableEvent(new QuestionnaireItemMoved()
                    {
                        PublicKey = Guid.Parse(chapter1Id),
                        GroupKey = Guid.Parse(chapter2Id),
                        TargetIndex = 0
                    }));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_count_be_equal_to_1 = () =>
            viewState.Chapters.Count.ShouldEqual(1);

        It should_not_questionnnaireInfoView_Chapters_contains_chapter1 = () =>
            viewState.Chapters.FirstOrDefault(chapter=>chapter.ChapterId == chapter1Id).ShouldBeNull();

        private static string chapter1Id = "33333333333333333333333333333333";
        private static string chapter2Id = "22222222222222222222222222222222";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
