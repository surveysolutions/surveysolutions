using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_content_and_image_declares_too_big_size : AttachmentServiceTestContext
    {
        [TestCase(4097, 10)]
        [TestCase(10, 4097)]
        [TestCase(5000, 5000)]
        public void should_throw_image_is_too_big_without_decoding_the_image(int width, int height)
        {
            var attachmentService = Create.AttachmentService();

            var exception = Assert.Throws<FormatException>(() =>
                attachmentService.SaveContent(contentId, "image/png", PngTestData.WithDeclaredSize(width, height)));

            Assert.That(exception!.Message, Is.EqualTo(ExceptionMessages.Attachments_uploaded_file_image_is_too_big));
        }

        [Test]
        public void should_throw_image_is_too_big_when_image_declares_huge_size()
        {
            var attachmentService = Create.AttachmentService();

            // 600 megapixels, about 2.4 GB of memory for a file of a few dozen bytes
            var exception = Assert.Throws<FormatException>(() =>
                attachmentService.SaveContent(contentId, "image/png", PngTestData.WithDeclaredSize(20_000, 30_000)));

            Assert.That(exception!.Message, Is.EqualTo(ExceptionMessages.Attachments_uploaded_file_image_is_too_big));
        }

        [Test]
        public void when_size_is_at_the_limit_but_content_is_missing_should_throw_file_is_not_image()
        {
            var attachmentService = Create.AttachmentService();

            var exception = Assert.Throws<FormatException>(() =>
                attachmentService.SaveContent(contentId, "image/png", PngTestData.WithDeclaredSize(4096, 10)));

            Assert.That(exception!.Message, Is.EqualTo(ExceptionMessages.Attachments_uploaded_file_is_not_image));
        }

        private const string contentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
    }
}
