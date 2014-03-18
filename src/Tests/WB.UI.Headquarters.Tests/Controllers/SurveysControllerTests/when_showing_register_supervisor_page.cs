using System.Web.Mvc;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_register_supervisor_page : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            var surveyViewFactory = Substitute.For<ISurveyViewFactory>();

            var surveyDetails = CreateSurveyDetailsView(surveyId, title);

            surveyViewFactory.GetDetailsView(surveyId).Returns(surveyDetails);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);
        };

        Because of = () =>
            result = controller.RegisterSupervisor(surveyId);

        It should_return_view_with_model_of_type_SupervisorModel = () =>
            ((ViewResult)result).Model.ShouldBeOfExactType(typeof(SupervisorModel));

        It should_return_view_with_model_with_SurveyId_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyId.ShouldEqual(surveyId);

        It should_return_view_with_model_with_SurveyTitle_set_in_surveyId = () =>
            ((SupervisorModel)((ViewResult)result).Model).SurveyTitle.ShouldEqual(title);

        private static ActionResult result;
        private static SurveysController controller;
        private static string surveyId = "survey-id";
        private static string title = "some title";
    }
}