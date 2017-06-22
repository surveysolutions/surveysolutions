using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_content : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentStorage.Store(Create.AttachmentContent(content: fileContent, contentType: contentType, contentId: attachmentContentId), attachmentContentId);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentContent = attachmentService.GetContent(attachmentContentId);

        [NUnit.Framework.Test] public void should_return_attachment_content_with_specified_properties () 
        {
            attachmentContent.ContentId.ShouldEqual(attachmentContentId);
            attachmentContent.ContentType.ShouldEqual(contentType);
            attachmentContent.Content.ShouldEqual(fileContent);
        }

        private static AttachmentContent attachmentContent;
        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
    }
}