using System.Linq;
using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_register_supervisor_page_and_survey_not_exists : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();

            surveyViewFactory.GetDetailsView(surveyId).Returns(null as SurveyDetailsView);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);
        };

        Because of = () =>
            result = controller.RegisterSupervisor(surveyId);

        It should_return_http_not_found_result = () =>
            result.ShouldBeOfExactType<HttpNotFoundResult>();

        private static ActionResult result;
        private static SurveysController controller;
        private static string surveyId = "survey-id";
        private static string title = "some title";
    }
}