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
            attachmentContentStorage.Store(Create.AttachmentContent(content: fileContent, contentType: contentType), attachmentContentId);
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId, attachmentContentId, questionnaireId: questionnaireId, fileName: fileName), attachmentId);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage, attachmentMetaStorage: attachmentMetaStorage);
        };

        Because of = () =>
            attachment = attachmentService.GetAttachmentWithContent(attachmentId);

        It should_return_attachment_with_specified_properties = () =>
        {
            attachment.FileName.ShouldEqual(fileName);
            attachment.AttachmentId.ShouldEqual(attachmentId.FormatGuid());
            attachment.ContentType.ShouldEqual(contentType);
            attachment.AttachmentContentId.ShouldEqual(attachmentContentId);
            attachment.Content.ShouldEqual(fileContent);
        };

        private static QuestionnaireAttachment attachment;
        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}