using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_QuestionDeleted_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1ChapterAnd1Question(chapterId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState = denormalizer.Update(viewState, CreatePublishableEvent(new QuestionDeleted()));

        It should_questionnnaireInfoView_QuestionsCount_be_0 = () =>
            viewState.QuestionsCount.ShouldEqual(0);

        private static string chapterId = "33333333333333333333333333333333";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
