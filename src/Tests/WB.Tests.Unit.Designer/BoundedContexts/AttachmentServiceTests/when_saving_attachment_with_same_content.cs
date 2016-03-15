using System;
using System.Drawing;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_with_same_content : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentMetaStorage
                .Setup(x => x.GetById(attachmentId.FormatGuid()))
                .Returns(Create.AttachmentMeta(attachmentId.FormatGuid(), contentHash: fileContentHash));

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage.Object, attachmentMetaStorage.Object);

            attachmentService = Create.AttachmentService();
            attachmentService.ImageFromStream = stream => image;
        };

        Because of = () =>
            attachmentService.SaveAttachmentContent(questionnaireId, attachmentId, AttachmentType.Image, contentType, fileContent, fileName);

        It should_not_save_or_update_anything_in_meta_storage = () =>
            attachmentMetaStorage.Verify(x => x.Store(Moq.It.IsAny<AttachmentMeta>(), Moq.It.IsAny<string>()), Times.Never);

        It should_not_save_or_update_anything_in_content_storage = () =>
            attachmentContentStorage.Verify(x => x.Store(Moq.It.IsAny<AttachmentContent>(), Moq.It.IsAny<string>()), Times.Never);


        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string fileContentHash = GetHash(fileContent);
        private static readonly Image image = new Bitmap(2, 3);
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string contentType = "image/png";
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> attachmentContentStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
        private static readonly Mock<IPlainStorageAccessor<AttachmentMeta>> attachmentMetaStorage = new Mock<IPlainStorageAccessor<AttachmentMeta>>();
    }
}