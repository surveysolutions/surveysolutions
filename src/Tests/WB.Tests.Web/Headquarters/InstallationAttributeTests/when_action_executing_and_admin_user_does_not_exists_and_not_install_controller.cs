using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_does_not_exists_and_not_install_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attribute = CreateInstallationAttribute();
            BecauseOf();
        }

        public void BecauseOf() =>
            attribute.OnActionExecuting(actionExecutingContext);

        [NUnit.Framework.Test] public void should_action_executing_result_be_type_of_RedirectToRouteResult () =>
            actionExecutingContext.Result.Should().BeOfType<RedirectToRouteResult>();

        [NUnit.Framework.Test] public void should_action_executing_result_route_values_contains_2_values () =>
            GetActionExecutingContextResult().RouteValues.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_action_executing_result_route_values_contains_controller_key () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("controller").Should().BeTrue();

        [NUnit.Framework.Test] public void should_action_executing_result_route_values_contains_action_key () =>
            GetActionExecutingContextResult().RouteValues.ContainsKey("action").Should().BeTrue();

        [NUnit.Framework.Test] public void should_action_executing_result_controller_route_value_be_equal_to_install () =>
            GetActionExecutingContextResult().RouteValues["controller"].Should().Be("Install");

        [NUnit.Framework.Test] public void should_action_executing_result_action_route_value_be_equal_to_finish () =>
            GetActionExecutingContextResult().RouteValues["action"].Should().Be("Finish");

        private static RedirectToRouteResult GetActionExecutingContextResult()
        {
            return actionExecutingContext.Result as RedirectToRouteResult;
        }

        private static InstallationFilter attribute;
        private static ActionExecutingContext actionExecutingContext = CreateFilterContext();
    }
}
