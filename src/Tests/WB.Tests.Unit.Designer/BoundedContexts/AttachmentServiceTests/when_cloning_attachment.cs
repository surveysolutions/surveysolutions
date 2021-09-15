using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_cloning_attachment : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            dbContext.AttachmentContents.Add(Create.AttachmentContent(contentId: attachmentContentId, content: fileContent, contentType: contentType));
            dbContext.AttachmentMetas.Add(Create.AttachmentMeta(attachmentId, attachmentContentId, questionnaireId: questionnaireId, fileName: fileName));

            dbContext.SaveChanges();

            attachmentService = Create.AttachmentService(dbContext);
            BecauseOf();
        }

        private void BecauseOf() => attachmentService.CloneMeta(attachmentId, newAttachmentId, newQuestionnaireId);

        [NUnit.Framework.Test] public void should_save_cloned_attachment_meta_with_specified_properties () 
        {
            var attachmentMeta = dbContext.AttachmentMetas.Find(newAttachmentId);
            attachmentMeta.FileName.Should().Be(fileName);
            attachmentMeta.QuestionnaireId.Should().Be(newQuestionnaireId);
            attachmentMeta.ContentId.Should().Be(attachmentContentId);
        }

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string fileName = "Attachment.PNG";
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly Guid questionnaireId =  Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid newQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid newAttachmentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly string contentType = "image/png";
        private static readonly DesignerDbContext dbContext = Create.InMemoryDbContext();
    }
}
