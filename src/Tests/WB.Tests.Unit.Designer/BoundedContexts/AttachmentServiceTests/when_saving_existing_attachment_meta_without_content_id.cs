using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_existing_attachment_meta_without_content_id : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentMetaStorage.Setup(x => x.GetById(attachmentId)).Returns(expectedAttachmentMeta);

            attachmentService = Create.AttachmentService(attachmentMetaStorage: attachmentMetaStorage.Object);
        };

        Because of = () =>
            attachmentService.SaveMeta(attachmentId, questionnaireId, null, fileName);

        It should_save_meta_storage = () =>
            attachmentMetaStorage.Verify(x => x.Store(expectedAttachmentMeta, attachmentId), Times.Once);

        It should_meta_contains_previous_content_id = () =>
        {
            expectedAttachmentMeta.ContentId.ShouldEqual(expectedAttachmentMeta.ContentId);
        };

        private static AttachmentService attachmentService;
        
        private static readonly Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string fileName = "image.png";
        private static readonly AttachmentMeta expectedAttachmentMeta = new AttachmentMeta
        {
            AttachmentId = attachmentId,
            FileName = "myfile.jpg",
            QuestionnaireId = Guid.Parse("33333333333333333333333333333333"),
            ContentId = "old content id"
        };
        private static readonly Mock<IPlainStorageAccessor<AttachmentMeta>> attachmentMetaStorage = new Mock<IPlainStorageAccessor<AttachmentMeta>>();
    }
}