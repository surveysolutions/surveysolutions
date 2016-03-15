using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage.Store(Create.AttachmentContent(content: fileContent, contentType: contentType), fileContentHash);
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId.FormatGuid(), fileContentHash, questionnaireId: questionnaireId.FormatGuid(), fileName: fileName), attachmentId.FormatGuid());

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage, attachmentMetaStorage);

            attachmentService = Create.AttachmentService();
        };

        Because of = () =>
            attachment = attachmentService.GetAttachment(attachmentId);

        It should_return_attachment_with_specified_properties = () =>
        {
            attachment.FileName.ShouldEqual(fileName);
            attachment.AttachmentId.ShouldEqual(attachmentId.FormatGuid());
            attachment.ContentType.ShouldEqual(contentType);
            attachment.AttachmentContentId.ShouldEqual(fileContentHash);
            attachment.Content.ShouldEqual(fileContent);
        };

        private static QuestionnaireAttachment attachment;
        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string fileContentHash = GetHash(fileContent);
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}