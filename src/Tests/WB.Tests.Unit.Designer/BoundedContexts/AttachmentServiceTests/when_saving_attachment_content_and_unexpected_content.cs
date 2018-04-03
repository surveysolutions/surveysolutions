using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_content_and_unexpected_content : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test] public void should_throw_format_exception () {
            attachmentContentStorage
                .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<AttachmentContent>, bool>>()))
                .Returns(false);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage.Object);

            Assert.Throws<FormatException>(() =>
                attachmentService.SaveContent(attachmentContentId, contentType, fileContent));
        }

        private static AttachmentService attachmentService;

        private static readonly byte[] fileContent = {255, 216, 255};
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> attachmentContentStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
    }
}
