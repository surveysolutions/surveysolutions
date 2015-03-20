using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_exists_and_install_controller : InstallationAttributeTestsContext
    {
        Establish context = () =>
        {
            var identityManagerMock = new Mock<IIdentityManager>();
            identityManagerMock.Setup(_ => _.GetUsersInRole(Moq.It.IsAny<string>())).Returns(new[] {"hq"});
            
            attribute = Create(identityManagerMock.Object);
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        attribute.OnActionExecuting(CreateFilterContext(new InstallController(null, null, null, null))));

        It should_exception_not_be_null = () =>
            exception.ShouldNotBeNull();

        It should_exception_be_type_of_HttpException = () =>
            exception.ShouldBeOfExactType<HttpException>();

        It should_exception_status_code_be_equal_to_404 = () =>
            ((HttpException)exception).GetHttpCode().ShouldEqual(404);

        private static InstallationAttribute attribute;
        private static Exception exception;
    }
}