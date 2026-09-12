using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_content_with_heic_content_type : AttachmentServiceTestContext
    {
        [TestCase("image/heic")]
        [TestCase("image/heif")]
        [TestCase("image/heic-sequence")]
        [TestCase("image/heif-sequence")]
        [TestCase("image/HEIC")]
        [TestCase("image/HEIF")]
        public void should_throw_format_exception_with_unsupported_content_message(string contentType)
        {
            var attachmentService = Create.AttachmentService();

            var ex = Assert.Throws<FormatException>(() =>
                attachmentService.SaveContent(attachmentContentId, contentType, fileContent));

            Assert.That(ex.Message, Is.EqualTo(ExceptionMessages.Attachments_Unsupported_content));
        }

        private static readonly byte[] fileContent = { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
    }
}
