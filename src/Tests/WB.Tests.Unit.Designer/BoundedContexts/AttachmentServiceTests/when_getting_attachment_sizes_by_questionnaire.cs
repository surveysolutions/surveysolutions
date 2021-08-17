using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_sizes_by_questionnaire : AttachmentServiceTestContext
    {
        [NUnit.Framework.Test]
        public async Task should_return_3_specified_attachment_sizes()
        {
            await attachmentMetaStorage.AttachmentMetas.AddRangeAsync(allAttachments);
            await attachmentMetaStorage.AttachmentContents.AddRangeAsync(allContents);
            await attachmentMetaStorage.SaveChangesAsync();

            attachmentService = Create.AttachmentService(attachmentMetaStorage);
            BecauseOf();

            expectedAttachmentSizes.Count.Should().Be(3);
            expectedAttachmentSizes[0].Size.Should().Be(100);
            expectedAttachmentSizes[1].Size.Should().Be(100);
            expectedAttachmentSizes[2].Size.Should().Be(50);
        }

        private void BecauseOf() =>
            expectedAttachmentSizes = attachmentService.GetAttachmentSizesByQuestionnaire(questionnaireId);

        private static AttachmentService attachmentService;
        private static List<AttachmentSize> expectedAttachmentSizes;
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly AttachmentContent[] allContents =
        {
            Create.AttachmentContent(contentId: "content 1", size: 100),
            Create.AttachmentContent(contentId: "content 2", size: 50),
            Create.AttachmentContent(contentId: "content 3", size: 20)
        };
        private static readonly AttachmentMeta[] allAttachments =
        {
            Create.AttachmentMeta(Guid.NewGuid(), allContents[0].ContentId, questionnaireId),
            Create.AttachmentMeta(Guid.NewGuid(), allContents[0].ContentId, questionnaireId),
            Create.AttachmentMeta(Guid.NewGuid(), allContents[1].ContentId, questionnaireId)
        };

        private static readonly DesignerDbContext attachmentMetaStorage = Create.InMemoryDbContext();
    }
}
