using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_existing_survey_details : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            surveyDetailsView = new SurveyDetailsView { SurveyId = surveyId, Name = "My survey" };

            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();
            surveyViewFactory.GetDetailsView(surveyId).Returns(surveyDetailsView);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);
        };

        Because of = () =>
            result = controller.Details(surveyId);

        It should_return_view = () =>
            result.ShouldBeOfExactType<ViewResult>();

        It should_return_view_model_containing_survey_details_view_provided_by_view_factory = () =>
            ((ViewResult)result).Model.ShouldEqual(surveyDetailsView);

        private static ActionResult result;
        private static SurveysController controller;
        private static SurveyDetailsView surveyDetailsView;
        private static string surveyId = "survey-id";
    }
}