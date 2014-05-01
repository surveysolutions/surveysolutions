using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_QuestionnaireUpdated_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            viewState = CreateQuestionnaireInfoView();
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState =
                denormalizer.Update(viewState, Create.QuestionnaireUpdatedEvent(questionnaireId:questionnaireId, questionnaireTitle: questionnaireTitle));

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
