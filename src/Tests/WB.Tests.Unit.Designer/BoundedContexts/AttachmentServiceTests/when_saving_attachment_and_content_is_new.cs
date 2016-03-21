using System;
using System.Drawing;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_and_content_is_new : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage.Store(Create.AttachmentContent(), oldContentHash);
            attachmentMetaStorage.Store(Create.AttachmentMeta(attachmentId.FormatGuid(), oldContentHash, questionnaireId.FormatGuid()), attachmentId.FormatGuid());

            Setup.ServiceLocatorForAttachmentService(attachmentContentStorage, attachmentMetaStorage);
            
            attachmentService = Create.AttachmentService();
            attachmentService.ImageFromStream = stream => image;
        };

        Because of = () =>
            attachmentService.SaveAttachmentContent(questionnaireId, attachmentId, attachmentContentId, contentType, fileContent, fileName);

        It should_save_attachment_meta = () =>
            attachmentMetaStorage.GetById(attachmentId.FormatGuid()).ShouldNotBeNull();

        It should_save_attachment_content = () =>
            attachmentContentStorage.GetById(attachmentContentId).ShouldNotBeNull();

        It should_delete_old_content = () =>
            attachmentContentStorage.GetById(oldContentHash).ShouldBeNull();

        It should_save_attachment_meta_with_specified_properties = () =>
        {
            var attachmentMeta = attachmentMetaStorage.GetById(attachmentId.FormatGuid());
            attachmentMeta.FileName.ShouldEqual(fileName);
            attachmentMeta.QuestionnaireId.ShouldEqual(questionnaireId.FormatGuid());
            attachmentMeta.AttachmentContentHash.ShouldEqual(attachmentContentId);
        };

        It should_save_attachment_content_with_specified_properties = () =>
        {
            var attachmentContent = attachmentContentStorage.GetById(attachmentContentId);
            attachmentContent.Content.ShouldBeTheSameAs(fileContent);
            attachmentContent.AttachmentContentHash.ShouldEqual(attachmentContentId);
            attachmentContent.ContentType.ShouldEqual(contentType);
            attachmentContent.Details.Height.ShouldEqual(3);
            attachmentContent.Details.Width.ShouldEqual(2);
        };

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string oldContentHash = "prev_hash";
        private static readonly Image image = new Bitmap(2, 3);
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly TestPlainStorage<AttachmentContent> attachmentContentStorage = new TestPlainStorage<AttachmentContent>();
        private static readonly TestPlainStorage<AttachmentMeta> attachmentMetaStorage = new TestPlainStorage<AttachmentMeta>();
    }
}