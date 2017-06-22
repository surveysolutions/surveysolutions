using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.UsersControllerTests
{
    public class when_user_logged_in_and_very_old_client_has_old_app_version : UsersControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateUserController();
            BecauseOf();
        }

        private void BecauseOf() => expectedException = Catch.Exception(() => controller.OldLogin());

        [NUnit.Framework.Test] public void should_response_code_be_UpgradeRequired () =>
            ((HttpResponseException)expectedException).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        private static Exception expectedException;
        private static UserController controller;
    }
}