using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.AttachmentsControllerTests
{
    public class when_getting_attachment_and_client_has_old_app_version : AttachmentsControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateAttachmentController();
            BecauseOf();
        }

        private void BecauseOf() => expectedException =
            Catch.Exception(() => controller.Get(version: ApiVersion.CurrentTesterProtocolVersion - 1, id: "hash of attachment"));

        [NUnit.Framework.Test] public void should_response_code_be_UpgradeRequired () =>
            ((HttpResponseException)expectedException).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        private static Exception expectedException;
        private static AttachmentController controller;
    }
}