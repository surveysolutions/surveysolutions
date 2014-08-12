using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_GroupDeleted_event_received_and_group_is_chapter : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.GroupDeletedEvent(groupId: chapterId));

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldBeEmpty();

        private static string chapterId = "33333333333333333333333333333333";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
