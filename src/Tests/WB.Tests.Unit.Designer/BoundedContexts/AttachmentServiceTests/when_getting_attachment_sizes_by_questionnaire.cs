using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_sizes_by_questionnaire : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            allAttachments.ForEach(attachment => attachmentMetaStorage.Store(attachment, attachment.AttachmentId));
            allContents.ForEach(content=>attachmentContentStorage.Store(content, content.ContentId));

            attachmentService = Create.AttachmentService(attachmentMetaStorage: attachmentMetaStorage, attachmentContentStorage: attachmentContentStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            expectedAttachmentSizes = attachmentService.GetAttachmentSizesByQuestionnaire(questionnaireId);

        [NUnit.Framework.Test] public void should_return_3_specified_attachment_sizes () 
        {
            expectedAttachmentSizes.Count.ShouldEqual(3);
            expectedAttachmentSizes[0].Size.ShouldEqual(100);
            expectedAttachmentSizes[1].Size.ShouldEqual(100);
            expectedAttachmentSizes[2].Size.ShouldEqual(50);
        }
        
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

        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
    }
}