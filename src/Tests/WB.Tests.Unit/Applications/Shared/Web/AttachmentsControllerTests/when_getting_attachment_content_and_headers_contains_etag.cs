using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Shared.Web.AttachmentsControllerTests
{
    internal class when_getting_attachment_content_and_headers_contains_etag
    {
        Establish context = () =>
        {
            mockOfAttachmentContentService.Setup(x => x.GetAttachmentContent(attachmentContentId)).Returns(expectedAttachmentContent);

            controller = Create.AttachmentsController(mockOfAttachmentContentService.Object);
            controller.Request = new HttpRequestMessage
            {
                Headers =
                {
                    IfNoneMatch = {new EntityTagHeaderValue($"\"{expectedAttachmentContent.ContentHash}\"", false)}
                }
            };
        };

        Because of = () => response = controller.Content(attachmentContentId);

        It should_return_NotNModified_response = () => response.StatusCode.ShouldEqual(HttpStatusCode.NotModified);

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