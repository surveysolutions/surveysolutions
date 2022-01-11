using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;


namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_already_existing_attachment_content : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test]
        public void should_not_save_content_to__storage()
        {
            attachmentContentStorage
                .AttachmentContents.Add(Create.AttachmentContent(contentId: attachmentContentId));
            attachmentContentStorage.SaveChanges();

            attachmentService = Create.AttachmentService(attachmentContentStorage);
            BecauseOf();
            attachmentContentStorage.AttachmentContents.Count().Should().Be(1);
        }

        private void BecauseOf() =>
            attachmentService.SaveContent(attachmentContentId, contentType, fileContent);

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly DesignerDbContext attachmentContentStorage = Create.InMemoryDbContext();
    }
}
