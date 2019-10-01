using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Shared.Web.AttachmentsControllerTests
{
    internal class when_getting_attachment_content_and_headers_contains_etag
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            mockOfAttachmentContentService.Setup(x => x.GetAttachmentContent(attachmentContentId)).Returns(expectedAttachmentContent);

            controller = Create.Controller.AttachmentsController(mockOfAttachmentContentService.Object);
            controller.Request = new HttpRequestMessage
            {
                Headers =
                {
                    IfNoneMatch = {new EntityTagHeaderValue($"\"{expectedAttachmentContent.ContentHash}\"", false)}
                }
            };
            BecauseOf();
        }

        public void BecauseOf() => response = controller.Content(attachmentContentId);

        [NUnit.Framework.Test] public void should_return_NotNModified_response () => response.StatusCode.Should().Be(HttpStatusCode.NotModified);

        private static AttachmentsController controller;
        private static string attachmentContentId = "Attahcment Content Id";
        private static readonly Mock<IAttachmentContentService> mockOfAttachmentContentService = new Mock<IAttachmentContentService>();
        private static HttpResponseMessage response;

        private static readonly AttachmentContent expectedAttachmentContent = new AttachmentContent
        {
            ContentHash = attachmentContentId,
            ContentType = "image/png",
            Content = new byte[] {1, 2, 3}
        };
    }
}
