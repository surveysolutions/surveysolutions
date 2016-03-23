using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_meta : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentMetaStorage.Setup(x => x.GetById(attachmentId)).Returns((AttachmentMeta)null);

            attachmentService = Create.AttachmentService(attachmentMetaStorage: attachmentMetaStorage.Object);
        };

        Because of = () =>
            attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName);

        It should_save_meta_storage = () =>
            attachmentMetaStorage.Verify(x => x.Store(Moq.It.IsAny<AttachmentMeta>(), attachmentId), Times.Once);
        

        private static AttachmentService attachmentService;

        
        private static readonly Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string attachmentContentId = "content id";
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string fileName = "image.png";
        private static readonly Mock<IPlainStorageAccessor<AttachmentMeta>> attachmentMetaStorage = new Mock<IPlainStorageAccessor<AttachmentMeta>>();
    }
}