using System.Net;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.Designer.Api.Tester.UsersControllerTests
{
    public class when_user_logged_in_and_client_has_old_app_version : UsersControllerTestsContext
    {
        [Test]
        public void should_response_code_be_UpgradeRequired()
        {
            var controller = CreateUserController();
            var expectedException =
                Assert.Throws<HttpResponseException>(() => controller.Login(version: ApiVersion.CurrentTesterProtocolVersion - 1));

            expectedException.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
        }
    }
}
