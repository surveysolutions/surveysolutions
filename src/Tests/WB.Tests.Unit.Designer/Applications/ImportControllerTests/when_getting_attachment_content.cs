using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Controllers.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_attachment_content : ImportControllerTestContext
    {
        [Test]
        public void should_return_specified_http_response()
        {
            Mock<IAttachmentService> mockOfAttachmentService = new Mock<IAttachmentService>();
            mockOfAttachmentService.Setup(x => x.GetContent(attachmentContentId)).Returns(expectedAttachmentContent);

            importController = CreateImportController(attachmentService: mockOfAttachmentService.Object);
            var response = BecauseOf();

            var result = (FileContentResult)response;
            Assert.That(result.FileContents, Is.EquivalentTo(expectedAttachmentContent.Content));
            Assert.That(result.ContentType, Is.EqualTo(expectedAttachmentContent.ContentType));
            Assert.That(result.EntityTag.ToString(), Is.EqualTo($"\"{expectedAttachmentContent.ContentId}\""));
        }

        private IActionResult BecauseOf() =>
             importController.AttachmentContent(attachmentContentId);

        private ImportV2Controller importController;
        private static string attachmentContentId = "Attahcment Content Id";
        private static readonly AttachmentContent expectedAttachmentContent = new AttachmentContent
        {
            ContentId = attachmentContentId,
            ContentType = "image/png",
            Content = new byte[] { 1, 2, 3 }
        };
    }
}
