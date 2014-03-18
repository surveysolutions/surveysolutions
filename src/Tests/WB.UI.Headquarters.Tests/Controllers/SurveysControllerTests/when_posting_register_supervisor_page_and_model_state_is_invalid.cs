using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_posting_register_supervisor_page_and_model_state_is_invalid : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyDetails = CreateSurveyDetailsView(surveyId, title);

            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();
            surveyViewFactory.GetDetailsView(surveyId).Returns(surveyDetails);

            model = CreateSupervisorModel(surveyId, login: login, password: password, confirmPassword: confirmPassword);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);

            controller.ModelState.AddModelError(string.Empty, "Some error");
        };

        Because of = () =>
            result = controller.RegisterSupervisor(surveyId, model);

        It should_return_view_with_model_of_type_SupervisorModel = () =>
            ((ViewResult)result).Model.ShouldBeOfExactType(typeof(SupervisorModel));

        It should_return_view_with_model_with_SurveyId_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyId.ShouldEqual(surveyId);

        It should_return_view_with_model_with_SurveyTitle_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyTitle.ShouldEqual(title);

        It should_return_view_with_model_with_Login_set_in_login = () =>
            ((SupervisorModel)((ViewResult)result).Model).Login.ShouldEqual(login);

        It should_return_view_with_model_with_Password_set_in_password = () =>
            ((SupervisorModel)((ViewResult)result).Model).Password.ShouldEqual(password);

        It should_return_view_with_model_with_ConfirmPassword_set_in_confirmPassword = () =>
            ((SupervisorModel)((ViewResult)result).Model).ConfirmPassword.ShouldEqual(confirmPassword);

        private static ActionResult result;
        private static SurveysController controller;
        private static SupervisorModel model;
        private static string surveyId = "survey-id";
        private static string title = "some title";
        private static string login = "Sidor";
        private static string password = "SidorsPassword";
        private static string confirmPassword = "SidorsPassword";
    }
}