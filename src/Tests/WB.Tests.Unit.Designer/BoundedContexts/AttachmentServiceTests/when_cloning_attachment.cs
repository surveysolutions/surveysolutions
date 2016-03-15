using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_cloning_attachment : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage.Store(Create.AttachmentContent(content: fileContent, contentType: contentType), fileContentHash);
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId.FormatGuid(), fileContentHash, questionnaireId: questionnaireId.FormatGuid(), fileName: fileName), attachmentId.FormatGuid());

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage, attachmentMetaStorage);

            attachmentService = Create.AttachmentService();
        };

        Because of = () =>
            attachmentService.CloneAttachmentMeta(attachmentId, newAttachmentId, newQuestionnaireId);

        It should_save_cloned_attachment_meta_with_specified_properties = () =>
        {
            var attachmentMeta = attachmentMetaStorage.GetById(newAttachmentId.FormatGuid());
            attachmentMeta.FileName.ShouldEqual(fileName);
            attachmentMeta.QuestionnaireId.ShouldEqual(newQuestionnaireId.FormatGuid());
            attachmentMeta.AttachmentContentHash.ShouldEqual(fileContentHash);
        };

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string fileContentHash = GetHash(fileContent);
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId =  Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid newQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid newAttachmentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}