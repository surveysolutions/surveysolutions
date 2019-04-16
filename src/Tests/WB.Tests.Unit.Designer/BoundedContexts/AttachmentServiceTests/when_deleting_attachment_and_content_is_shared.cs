using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;


namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_deleting_attachment_and_content_is_shared : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentStorage.Add(Create.AttachmentContent(contentId: contentHash));

            attachmentContentStorage.Add(Create.AttachmentMeta(attachmentId, contentHash, questionnaireId: questionnaireId));
            attachmentContentStorage.Add(Create.AttachmentMeta(otherAttachmentId, contentHash, otherQuestionnaireId));

            attachmentService = Create.AttachmentService();
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentService.DeleteAllByQuestionnaireId(questionnaireId);

        [NUnit.Framework.Test] public void should_delete_attachment_meta () =>
            attachmentContentStorage.AttachmentMetas.Find(attachmentId).Should().BeNull();

        [NUnit.Framework.Test] public void should_not_delete_attachment_content () =>
            attachmentContentStorage.AttachmentContents.Find(contentHash).Should().NotBeNull();

        private static AttachmentService attachmentService;
        private static readonly string contentHash = "prev_hash";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid otherQuestionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid otherAttachmentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly DesignerDbContext attachmentContentStorage = Create.InMemoryDbContext();
    }
}
