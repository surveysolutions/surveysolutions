using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_deleting_attachment_and_content_is_shared : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage.Store(Create.AttachmentContent(), contentHash);

            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId.FormatGuid(), contentHash, questionnaireId: questionnaireId.FormatGuid()), attachmentId.FormatGuid());
            attachmentMetaStorage.Store(Create.AttachmentMeta(otherAttachmentId, contentHash), otherAttachmentId);

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage, attachmentMetaStorage);

            attachmentService = Create.AttachmentService();
        };

        Because of = () =>
            attachmentService.Delete(attachmentId);

        It should_delete_attachment_meta = () =>
            attachmentMetaStorage.GetById(attachmentId.FormatGuid()).ShouldBeNull();

        It should_not_delete_attachment_content = () =>
            attachmentContentStorage.GetById(contentHash).ShouldNotBeNull();

        private static AttachmentService attachmentService;
        private static readonly string contentHash = "prev_hash";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string otherAttachmentId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}