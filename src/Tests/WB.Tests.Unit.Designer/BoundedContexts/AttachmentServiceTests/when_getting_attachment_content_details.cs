using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_content_details : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentStorage.Store(
                Create.AttachmentContent(content: fileContent, contentType: contentType, contentId: attachmentContentId,
                    size: fileContent.Length, details: contentDetails), attachmentContentId);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentContent = attachmentService.GetContentDetails(attachmentContentId);

        [NUnit.Framework.Test] public void should_return_attachment_content_with_specified_properties () 
        {
            attachmentContent.ContentId.ShouldEqual(attachmentContentId);
            attachmentContent.ContentType.ShouldEqual(contentType);
            attachmentContent.Size.ShouldEqual(fileContent.Length);
            attachmentContent.Details.Height.ShouldEqual(contentDetails.Height);
            attachmentContent.Details.Width.ShouldEqual(contentDetails.Width);
        }

        private static AttachmentContent attachmentContent;
        private static AttachmentService attachmentService;
        private static readonly AttachmentDetails contentDetails = new AttachmentDetails
        {
            Height = 10,
            Width = 20
        };
        private static readonly byte[] fileContent = { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
    }
}