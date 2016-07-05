﻿using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.UsersControllerTests
{
    public class when_user_logged_in_and_client_has_old_app_version : UsersControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateUserController();
        };

        Because of = () => expectedException =
            Catch.Exception(() => controller.Login(version: ApiVersion.CurrentTesterProtocolVersion - 1));

        It should_response_code_be_UpgradeRequired = () =>
            ((HttpResponseException)expectedException).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        private static Exception expectedException;
        private static UserController controller;
    }
}