using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewDenormalizerTests
{
    internal class when_NewQuestionnaireCreated_event_received : QuestionnaireInfoViewDenormalizerTestContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewState = denormalizer.Create(
                CreatePublishableEvent(new NewQuestionnaireCreated()
                {
                    PublicKey = new Guid(questionnaireId),
                    Title = questionnaireTitle
                }, new Guid(questionnaireId)));

        It should_questionnnaireInfoView_QuestionnaireId_be_equal_to_questionnaireId = () =>
            viewState.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_questionnnaireInfoView_Title_be_equal_to_questionnaireTitle = () =>
            viewState.Title.ShouldEqual(questionnaireTitle);

        It should_questionnnaireInfoView_Chapters_not_be_null = () =>
            viewState.Chapters.ShouldNotBeNull();

        private static string questionnaireId = "33333333333333333333333333333333";
        private static string questionnaireTitle = "questionnaire title";
        
        private static QuestionnaireInfoViewDenormalizer denormalizer;
        private static QuestionnaireInfoView viewState;
    }
}
