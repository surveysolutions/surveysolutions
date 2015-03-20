using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_does_not_exists_and_not_install_controller : InstallationAttributeTestsContext
    {
        Establish context = () =>
        {
            var identityManagerMock = new Mock<IIdentityManager>();
            identityManagerMock.Setup(_ => _.GetUsersInRole(Moq.It.IsAny<string>())).Returns(new string[0]);
            
            attribute = Create(identityManagerMock.Object);
        };

        Because of = () =>
            attribute.OnActionExecuting(actionExecutingContext);

        It should_action_executing_result_be_type_of_RedirectToRouteResult = () =>
            actionExecutingContext.Result.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_action_executing_result_route_values_contains_2_values = () =>
            GetActionExecutingContextResult().RouteValues.Count.ShouldEqual(2);

        It should_action_executing_result_route_values_contains_controller_key = () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("controller");

        It should_action_executing_result_route_values_contains_action_key = () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("action");

        It should_action_executing_result_controller_route_value_be_equal_to_install = () =>
            GetActionExecutingContextResult().RouteValues["controller"].ShouldEqual("Install");

        It should_action_executing_result_action_route_value_be_equal_to_finish = () =>
            GetActionExecutingContextResult().RouteValues["action"].ShouldEqual("Finish");

        private static RedirectToRouteResult GetActionExecutingContextResult()
        {
            return actionExecutingContext.Result as RedirectToRouteResult;
        }

        private static InstallationAttribute attribute;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext();
    }
}