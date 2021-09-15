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
    internal class when_saving_attachment_meta : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentService = Create.AttachmentService(attachmentMetaStorage);
            BecauseOf();
        }

        private void BecauseOf()
        {
            attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName);
            attachmentMetaStorage.SaveChanges();
        }

        [NUnit.Framework.Test] public void should_save_meta_storage () =>
            attachmentMetaStorage.AttachmentMetas.Count().Should().Be(1);

        private static AttachmentService attachmentService;
        private static readonly Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string attachmentContentId = "content id";
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string fileName = "image.png";
        private static readonly DesignerDbContext attachmentMetaStorage = Create.InMemoryDbContext();
    }
}
