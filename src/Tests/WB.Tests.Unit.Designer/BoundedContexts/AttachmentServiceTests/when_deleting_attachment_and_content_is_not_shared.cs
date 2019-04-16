using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_deleting_attachment_and_content_is_not_shared : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            attachmentContentStorage.Add(Create.AttachmentContent(contentId: attachmentId.FormatGuid()));
            attachmentContentStorage.Add(Create.AttachmentMeta(attachmentId, contentHash, questionnaireId: questionnaireId));
            attachmentContentStorage.SaveChanges();

            attachmentService = Create.AttachmentService();
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentService.DeleteAllByQuestionnaireId(questionnaireId);

        [NUnit.Framework.Test]
        public void should_delete_attachment_meta() =>
            attachmentContentStorage.AttachmentMetas.Find(attachmentId).Should().BeNull();

        [NUnit.Framework.Test]
        public void should_delete_attachment_content() =>
            attachmentContentStorage.AttachmentContents.Find(contentHash).Should().BeNull();

        private static AttachmentService attachmentService;
        private static readonly string contentHash = "prev_hash";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly DesignerDbContext attachmentContentStorage = Create.InMemoryDbContext();
    }
}
