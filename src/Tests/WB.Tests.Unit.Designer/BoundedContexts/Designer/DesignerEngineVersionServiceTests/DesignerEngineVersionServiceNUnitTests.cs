using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService(
            IAttachmentService attachments = null)
        {
            return new DesignerEngineVersionService(attachments ?? Mock.Of<IAttachmentService>());
        }

        [Test]
        public void should_return_version_24_when_non_image_attachment_exists()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            var contentId = "contentId";
            questionnaire.Attachments.Add(Create.Attachment(Id.gA, contentId: contentId));

            var attachmentContent = Create.AttachmentContent(contentType: "video/mp4", contentId: contentId);

            var attachmentService = Mock.Of<IAttachmentService>(x => x.GetContent(contentId) == attachmentContent);

            var service = this.CreateDesignerEngineVersionService(attachmentService);
 
            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
 
            Assert.That(contentVersion, Is.EqualTo(24));
        }
    }
}
