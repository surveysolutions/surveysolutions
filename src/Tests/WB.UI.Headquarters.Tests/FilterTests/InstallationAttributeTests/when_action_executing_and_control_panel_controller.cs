using System;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_control_panel_controller : InstallationAttributeTestsContext
    {
        Establish context = () =>
        {
            var identityManagerMock = new Mock<IIdentityManager>();
            identityManagerMock.Setup(_ => _.GetUsersInRole(Moq.It.IsAny<string>())).Throws(new Exception());

            attribute = Create(identityManagerMock.Object);
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        attribute.OnActionExecuting(CreateFilterContext(new ControlPanelController(null, null))));

        It should_exception_be_null = () =>
            exception.ShouldBeNull();

        private static InstallationAttribute attribute;
        private static Exception exception;
    }
}