using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_not_existing_survey_details : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();
            surveyViewFactory.GetDetailsView(surveyId).Returns(null as SurveyDetailsView);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);
        };

        Because of = () =>
            result = controller.Details(surveyId);

        It should_return_http_not_found_result = () =>
            result.ShouldBeOfExactType<HttpNotFoundResult>();

        private static ActionResult result;
        private static SurveysController controller;
        private static string surveyId = "survey-id";
    }
}