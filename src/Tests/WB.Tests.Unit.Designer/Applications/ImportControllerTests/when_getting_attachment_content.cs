using System.Linq;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_attachment_content : ImportControllerTestContext
    {
        Establish context = () =>
        {
            mockOfAttachmentService.Setup(x => x.GetAttachmentContent(attachmentContentId)).Returns(expectedAttachmentContent);

            importController = CreateImportController(attachmentService: mockOfAttachmentService.Object);
        };

        Because of = () =>
            response = importController.AttachmentContent(attachmentContentId);

        It should_return_specified_http_response = () =>
        {
            response.Content.ReadAsByteArrayAsync().Result.SequenceEqual(expectedAttachmentContent.Content);
            response.Content.Headers.ContentType.MediaType.ShouldEqual(expectedAttachmentContent.ContentType);
            response.Headers.ETag.Tag.ShouldEqual($"\"{expectedAttachmentContent.AttachmentContentHash}\"");
        };

        private static ImportV2Controller importController;
        private static string attachmentContentId = "Attahcment Content Id";
        private static readonly Mock<IAttachmentService> mockOfAttachmentService = new Mock<IAttachmentService>();
        private static HttpResponseMessage response;

        private static readonly AttachmentContent expectedAttachmentContent = new AttachmentContent
        {
            AttachmentContentHash = attachmentContentId,
            ContentType = "image/png",
            Content = new byte[] {1, 2, 3}
        };
    }
}