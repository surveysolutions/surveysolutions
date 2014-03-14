using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.SurveysControllerTests
{
    internal class when_showing_all_survey_list : SurveysControllerTestsContext
    {
        Establish context = () =>
        {
            allSurveyLineViews = new[] { new SurveyLineView { SurveyId = "id1" }, new SurveyLineView { SurveyId = "id2" } };

            var surveyViewFactory = Mock.Of<ISurveyViewFactory>(factory
                => factory.GetAllLineViews() == allSurveyLineViews);

            controller = CreateSurveysController(surveyViewFactory: surveyViewFactory);
        };

        Because of = () =>
            result = controller.Index();

        It should_return_view = () =>
            result.ShouldBeOfExactType<ViewResult>();

        It should_return_view_model_containing_all_survey_line_views_provided_by_view_factory = () =>
            ((ViewResult)result).Model.ShouldEqual(allSurveyLineViews);

        private static ActionResult result;
        private static SurveysController controller;
        private static SurveyLineView[] allSurveyLineViews;
    }
}