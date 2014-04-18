using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class when_getting_questionnaire_and_questionnaire_exists : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            var questionnaireInfoView = CreateQuestionnaireInfoView();

            var questionnaireInfoViewFactory =
                Mock.Of<IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView>>(
                    x => x.Load(Moq.It.IsAny<QuestionnaireInfoViewInputModel>()) == questionnaireInfoView);

            controller = CreateQuestionnaireController(questionnaireInfoViewFactory: questionnaireInfoViewFactory);
        };

        Because of = () =>
            result = controller.Get(questionnaireId);

        It should_questionnaire_not_be_null = () =>
            result.ShouldNotBeNull();

        private static QuestionnaireController controller;
        private static QuestionnaireInfoView result;
        private static string questionnaireId = "22222222222222222222222222222222";
    }
}