using System.Net;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.Api.Tester.UsersControllerTests
{
    public class when_user_logged_in_and_very_old_client_has_old_app_version : UsersControllerTestsContext
    {
        [Test]
        public void context()
        {
            var controller = CreateUserController();
            var expectedException = Assert.Throws<HttpResponseException>(() => controller.OldLogin());

            expectedException.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
        }
    }
}
