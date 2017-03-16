﻿using System;
using Machine.Specifications;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_does_not_exists_and_install_controller : InstallationAttributeTestsContext
    {
        Establish context = () =>
        {
            attribute = CreateInstallationAttribute();
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        attribute.OnActionExecuting(CreateFilterContext(new InstallController(null, null, null, null))));

        It should_exception_be_null = () =>
            exception.ShouldBeNull();

        private static InstallationAttribute attribute;
        private static Exception exception;
    }
}