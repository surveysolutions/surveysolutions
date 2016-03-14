using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_GroupUpdated_event_received_and_group_is_chapter : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoView();
            denormalizer = CreateDenormalizer(viewState);
            denormalizer.Update(viewState,
                CreatePublishableEvent(new NewGroupAdded() {PublicKey = Guid.Parse(chapterId)}));
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupUpdatedEvent(groupId: chapterId, groupTitle: chapterTitle));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_questionnnaireInfoView_Chapters_contains_1_chapter = () =>
            viewState.Chapters.Count.ShouldEqual(1);

        It should_questionnnaireInfoView_first_chapter_title_be_equal_to_chapterTitle = () =>
            viewState.Chapters[0].Title.ShouldEqual(chapterTitle);

        private static string chapterId = "33333333333333333333333333333333";
        private static string chapterTitle = "chapter title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
