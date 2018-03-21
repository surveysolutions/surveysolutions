using FluentAssertions;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_summary_for_map_point_and_input_view_model_is_null : InterviewApiControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateController();
            BecauseOf();
        }

        public void BecauseOf() =>
            viewModel = controller.InterviewSummaryForMapPoint(null);

        [NUnit.Framework.Test] public void should_view_model_be_null () =>
            viewModel.Should().BeNull();

        private static InterviewApiController controller;
        private static InterviewSummaryForMapPointView viewModel;
    }
}
