using System.Linq;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Shared.Web.AttachmentsControllerTests
{
    internal class when_getting_attachment_content
    {
        Establish context = () =>
        {
            mockOfAttachmentContentService.Setup(x => x.GetAttachmentContent(attachmentContentId)).Returns(expectedAttachmentContent);

            controller = Create.AttachmentsController(mockOfAttachmentContentService.Object);
        };

        Because of = () =>
            response = controller.Content(attachmentContentId);

        It should_return_specified_http_response = () =>
        {
            response.Content.ReadAsByteArrayAsync().Result.SequenceEqual(expectedAttachmentContent.Content);
            response.Content.Headers.ContentType.MediaType.ShouldEqual(expectedAttachmentContent.ContentType);
            response.Headers.ETag.Tag.ShouldEqual($"\"{expectedAttachmentContent.ContentHash}\"");
        };

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