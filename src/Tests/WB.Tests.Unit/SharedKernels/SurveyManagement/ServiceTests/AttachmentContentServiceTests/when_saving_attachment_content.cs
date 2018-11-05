using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_saving_attachment_content 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentService = Create.Service.AttachmentContentService(mockOfAttachmentContentPlainStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            attachmentContentService.SaveAttachmentContent(contentHash, contentType, content);

        [NUnit.Framework.Test] public void should_store_attachment_content_to_plain_storage () =>
            mockOfAttachmentContentPlainStorage.Verify(x=>x.Store(Moq.It.IsAny<AttachmentContent>(), contentHash), Times.Once);

        private static AttachmentContentService attachmentContentService;
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> mockOfAttachmentContentPlainStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
        private static string contentHash = "content hash";
        private static string contentType = "image/png";
        private static readonly byte[] content = new byte[] {1, 2, 3};
    }
}
