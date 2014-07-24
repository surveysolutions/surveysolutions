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
                denormalizer.Update(viewState, Create.QuestionnaireUpdatedEvent(questionnaireId:questionnaireId, questionnaireTitle: questionnaireTitle, isPublic: true));

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        It should_update_IsPublic_property = () => viewState.IsPublic.ShouldBeTrue();

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
