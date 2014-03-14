using System.Web.Mvc;
using Machine.Specifications;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_empty_start_new_survey_form : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateSurveysController();
        };

        Because of = () =>
            result = controller.StartNew();

        It should_return_view = () =>
            result.ShouldBeOfExactType<ViewResult>();

        private static ActionResult result;
        private static SurveysController controller;
    }
}