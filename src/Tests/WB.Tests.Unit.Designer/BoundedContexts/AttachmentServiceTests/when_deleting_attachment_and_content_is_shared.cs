using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;


namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_deleting_attachment_and_content_is_shared : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentStorage.Store(Create.AttachmentContent(), contentHash);

            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId, contentHash, questionnaireId: questionnaireId), attachmentId);
            attachmentMetaStorage.Store(Create.AttachmentMeta(otherAttachmentId, contentHash, otherQuestionnaireId), otherAttachmentId);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage, attachmentMetaStorage: attachmentMetaStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentService.DeleteAllByQuestionnaireId(questionnaireId);

        [NUnit.Framework.Test] public void should_delete_attachment_meta () =>
            attachmentMetaStorage.GetById(attachmentId).ShouldBeNull();

        [NUnit.Framework.Test] public void should_not_delete_attachment_content () =>
            attachmentContentStorage.GetById(contentHash).ShouldNotBeNull();

        private static AttachmentService attachmentService;
        private static readonly string contentHash = "prev_hash";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid otherQuestionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid otherAttachmentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}