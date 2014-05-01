using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_NewQuestionAdded_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoViewWith1Chapter(chapterId);
            denormalizer = CreateDenormalizer(viewState);
        };

        Because of = () =>
            viewState = denormalizer.Update(viewState, Create.NewQuestionAddedEvent());

        It should_questionnnaireInfoView_QuestionsCount_be_1 = () =>
            viewState.QuestionsCount.ShouldEqual(1);

        private static string chapterId = "33333333333333333333333333333333";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
