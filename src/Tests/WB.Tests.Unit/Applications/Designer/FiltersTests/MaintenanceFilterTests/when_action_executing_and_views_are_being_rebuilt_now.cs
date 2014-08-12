using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.FiltersTests.MaintenanceFilterTests
{
    internal class when_action_executing_and_views_are_being_rebuilt_now : MaintenanceFilterTestsContext
    {
        Establish context = () =>
        {
            var readSideStatusServiceMock = new Mock<IReadSideStatusService>();
            readSideStatusServiceMock.Setup(_ => _.AreViewsBeingRebuiltNow()).Returns(true);

            filter = Create(readSideStatusServiceMock.Object);
        };

        Because of = () =>
            filter.OnActionExecuting(actionExecutingContext);

        It should_action_executing_result_be_type_of_RedirectToRouteResult = () =>
            actionExecutingContext.Result.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_action_executing_result_route_values_contains_3_values = () =>
            GetActionExecutingContextResult().RouteValues.Count.ShouldEqual(3);

        It should_action_executing_result_route_values_contains_controller_key = () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("controller");

        It should_action_executing_result_route_values_contains_action_key = () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("action");

        It should_action_executing_result_route_values_contains_returnUrl_key = () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("returnUrl");

        It should_action_executing_result_controller_route_value_be_equal_to_Maintenance = () =>
            GetActionExecutingContextResult().RouteValues["controller"].ShouldEqual("Maintenance");

        It should_action_executing_result_action_route_value_be_equal_to_WaitForReadLayerRebuild = () =>
            GetActionExecutingContextResult().RouteValues["action"].ShouldEqual("WaitForReadLayerRebuild");

        It should_action_executing_result_returnUrl_route_value_be_equal_to_specified_returnUrl = () =>
            GetActionExecutingContextResult().RouteValues["returnUrl"].ShouldEqual(returnUrl);

        private static RedirectToRouteResult GetActionExecutingContextResult()
        {
            return actionExecutingContext.Result as RedirectToRouteResult;
        }

        private static MaintenanceFilter filter;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext(returnUrl: returnUrl);
        private const string returnUrl = "http://some.url/";
    }
}