using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_deleting_attachment_and_content_is_not_shared : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test]
        public void should_delete_attachment_meta_with_content()
        {
            attachmentContentStorage.AttachmentContents.Add(Create.AttachmentContent(contentId: contentHash));
            attachmentContentStorage.AttachmentMetas.Add(Create.AttachmentMeta(attachmentId, contentHash, questionnaireId: questionnaireId));
            attachmentContentStorage.SaveChanges();

            attachmentService = Create.AttachmentService(attachmentContentStorage);
            BecauseOf();

            attachmentContentStorage.AttachmentMetas.Find(attachmentId).Should().BeNull();
            attachmentContentStorage.AttachmentContents.Find(contentHash).Should().BeNull();
        }

        private void BecauseOf() =>
            attachmentService.DeleteAllByQuestionnaireId(questionnaireId);

        private static AttachmentService attachmentService;
        private static readonly string contentHash = "prev_hash";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly DesignerDbContext attachmentContentStorage = Create.InMemoryDbContext();
    }
}
