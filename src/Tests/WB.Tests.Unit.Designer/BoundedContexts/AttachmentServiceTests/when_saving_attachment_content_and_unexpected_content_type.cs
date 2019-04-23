using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;


namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_content_and_unexpected_content_type : AttachmentServiceTestContext
    {
        [Test]
        public void should_throw_format_exception()
        {

            var attachmentContentStorage = Create.InMemoryDbContext();

            attachmentService = Create.AttachmentService(attachmentContentStorage);

            Assert.Throws<FormatException>(() => attachmentService.SaveContent(attachmentContentId, contentType, fileContent));
        }

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = { 255, 216, 255 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "text/txt";
    }
}
