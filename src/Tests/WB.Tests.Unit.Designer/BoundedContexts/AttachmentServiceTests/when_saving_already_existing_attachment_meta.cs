using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_already_existing_attachment_meta : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            
            attachmentMetaStorage.AttachmentMetas.Add(expectedAttachmentMeta);
            attachmentMetaStorage.SaveChanges();

            attachmentService = Create.AttachmentService(attachmentMetaStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName);

        [NUnit.Framework.Test] public void should_save_meta_storage () =>
            attachmentMetaStorage.AttachmentMetas.First().AttachmentId.Should().Be(attachmentId);

        [NUnit.Framework.Test] public void should_meta_have_updated_properties () 
        {
            expectedAttachmentMeta.ContentId.Should().Be(attachmentContentId);
            expectedAttachmentMeta.FileName.Should().Be(fileName);
        }

        private static AttachmentService attachmentService;
        
        private static readonly Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string attachmentContentId = "content id";
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string fileName = "image.png";
        private static readonly AttachmentMeta expectedAttachmentMeta = new AttachmentMeta
        {
            AttachmentId = attachmentId,
            FileName = "myfile.jpg",
            QuestionnaireId = Guid.Parse("33333333333333333333333333333333"),
            ContentId = "old content id"
        };
        private static readonly DesignerDbContext attachmentMetaStorage = Create.InMemoryDbContext();
    }
}
