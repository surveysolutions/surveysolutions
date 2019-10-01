using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Shared.Web.AttachmentsControllerTests
{
    internal class when_getting_attachment_content
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            mockOfAttachmentContentService.Setup(x => x.GetAttachmentContent(attachmentContentId)).Returns(expectedAttachmentContent);

            controller = Create.Controller.AttachmentsController(mockOfAttachmentContentService.Object);
            controller.Request = new HttpRequestMessage {Headers = {IfNoneMatch = {}}};
            BecauseOf();
        }

        public void BecauseOf() =>
            response = controller.Content(attachmentContentId);

        [NUnit.Framework.Test] public void should_return_specified_http_response () 
        {
            response.Content.ReadAsByteArrayAsync().Result.SequenceEqual(expectedAttachmentContent.Content);
            response.Content.Headers.ContentType.MediaType.Should().Be(expectedAttachmentContent.ContentType);
            response.Headers.ETag.Tag.Should().Be($"\"{expectedAttachmentContent.ContentHash}\"");
        }

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
