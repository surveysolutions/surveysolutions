using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_NewQuestionnaireCreated_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState = denormalizer.Update(null, Create.NewQuestionnaireCreatedEvent(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle, isPublic: true));

        It should_questionnnaireInfoView_QuestionnaireId_be_equal_to_questionnaireId = () =>
            viewState.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        It should_fill_is_public_flag = () => viewState.IsPublic.ShouldBeTrue();

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
