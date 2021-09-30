using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_content_details : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test] public void should_return_attachment_content_with_specified_properties() {
            attachmentContentStorage.Add(
                Create.AttachmentContent(content: fileContent, contentType: contentType, contentId: attachmentContentId,
                    size: fileContent.Length, details: contentDetails));

            attachmentContentStorage.SaveChanges();
            attachmentService = Create.AttachmentService(attachmentContentStorage);

            BecauseOf();

            attachmentContent.ContentId.Should().Be(attachmentContentId);
            attachmentContent.ContentType.Should().Be(contentType);
            attachmentContent.Size.Should().Be(fileContent.Length);
            attachmentContent.Details.Height.Should().Be(contentDetails.Height);
            attachmentContent.Details.Width.Should().Be(contentDetails.Width);
        }

        private void BecauseOf() =>
            attachmentContent = attachmentService.GetContentDetails(attachmentContentId);

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
        private static readonly DesignerDbContext attachmentContentStorage = Create.InMemoryDbContext();
    }
}
