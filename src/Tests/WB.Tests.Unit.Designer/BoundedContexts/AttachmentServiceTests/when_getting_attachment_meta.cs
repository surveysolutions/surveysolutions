using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_meta : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test]
        public void should_return_attachment_meta_with_specified_properties()
        {
            attachmentMetaStorage.Add(Create.AttachmentMeta(attachmentId, attachmentContentId, questionnaireId: questionnaireId, fileName: fileName));
            attachmentMetaStorage.SaveChanges();

            attachmentService = Create.AttachmentService(attachmentMetaStorage);
            BecauseOf();

            attachment.FileName.Should().Be(fileName);
            attachment.AttachmentId.Should().Be(attachmentId);
            attachment.ContentId.Should().Be(attachmentContentId);
            attachment.QuestionnaireId.Should().Be(questionnaireId);
        }

        private void BecauseOf() =>
            attachment = attachmentService.GetAttachmentMeta(attachmentId);

        private static AttachmentMeta attachment;
        private static AttachmentService attachmentService;
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly DesignerDbContext attachmentMetaStorage = Create.InMemoryDbContext();
    }
}
