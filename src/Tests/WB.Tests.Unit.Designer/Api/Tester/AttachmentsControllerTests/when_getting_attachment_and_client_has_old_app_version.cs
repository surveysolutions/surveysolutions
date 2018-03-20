using System.Net;
using System.Web.Http;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.Designer.Api.Tester.AttachmentsControllerTests
{
    public class when_getting_attachment_and_client_has_old_app_version : AttachmentsControllerTestsContext
    {
        [Test]
        public void should_response_code_be_UpgradeRequired()
        {
            var controller = CreateAttachmentController();
            var expectedException = Assert.Throws<HttpResponseException>(
                () => controller.Get(version: ApiVersion.CurrentTesterProtocolVersion - 1, id: "hash of attachment"));

            expectedException.Response.StatusCode.Should().Be(HttpStatusCode.UpgradeRequired);
        }
    }
}
