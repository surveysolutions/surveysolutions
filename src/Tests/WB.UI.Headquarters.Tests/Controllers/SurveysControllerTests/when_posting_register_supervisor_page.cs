using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_posting_register_supervisor_page : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyDetails = CreateSurveyDetailsView(surveyId, title);

            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();
            surveyViewFactory.GetDetailsView(surveyId).Returns(surveyDetails);

            model = CreateSupervisorModel(surveyId);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);

            controller.ModelState.Clear();
        };

        Because of = () =>
            result = controller.RegisterSupervisor(surveyId, model);

        It should_redirect_to_Details_action = () =>
            result.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_set_action_to__Details__in_route_values = () =>
            ((RedirectToRouteResult) result).RouteValues["action"].ShouldEqual("Details");

        It should_set_controller_to_null_in_route_values = () =>
            ((RedirectToRouteResult)result).RouteValues["controller"].ShouldBeNull();

        It should_set_id_to__surveyId__in_route_values = () =>
            ((RedirectToRouteResult)result).RouteValues["id"].ShouldEqual(surveyId);

        private static ActionResult result;
        private static SurveysController controller;
        private static SupervisorModel model;
        private static string surveyId = "11111111111111111111111111111111";
        private static string title = "some title";
        private static string exceptionMessage = "some message";
    }
}