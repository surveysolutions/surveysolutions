using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_and_meta_is_absent : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage, attachmentMetaStorage: attachmentMetaStorage);
        };

        Because of = () =>
            attachment = attachmentService.GetAttachmentWithContent(attachmentId);

        It should_return_null = () =>
            attachment.ShouldBeNull();
        
        private static QuestionnaireAttachment attachment;
        private static AttachmentService attachmentService;
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}