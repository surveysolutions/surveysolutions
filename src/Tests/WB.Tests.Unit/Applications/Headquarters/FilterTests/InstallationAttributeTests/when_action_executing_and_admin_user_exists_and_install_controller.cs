﻿using System;
using System.Web;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_exists_and_install_controller : InstallationAttributeTestsContext
    {
        Establish context = () =>
        {
            attribute = CreateInstallationAttribute(Create.Storage.UserRepository(Create.Entity.HqUser(role: UserRoles.Administrator)));
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