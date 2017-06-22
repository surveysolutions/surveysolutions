using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_meta : AttachmentServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId, attachmentContentId, questionnaireId: questionnaireId, fileName: fileName), attachmentId);

            attachmentService = Create.AttachmentService(attachmentMetaStorage: attachmentMetaStorage);
            BecauseOf();
        }

        private void BecauseOf() =>
            attachment = attachmentService.GetAttachmentMeta(attachmentId);

        [NUnit.Framework.Test] public void should_return_attachment_meta_with_specified_properties () 
        {
            attachment.FileName.ShouldEqual(fileName);
            attachment.AttachmentId.ShouldEqual(attachmentId);
            attachment.ContentId.ShouldEqual(attachmentContentId);
            attachment.QuestionnaireId.ShouldEqual(questionnaireId);
        }

        private static AttachmentMeta attachment;
        private static AttachmentService attachmentService;
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}