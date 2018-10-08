using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_deleting_attachment_content 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContentService = Create.Service.AttachmentContentService(mockOfAttachmentContentPlainStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            attachmentContentService.DeleteAttachmentContent(contentHash);

        [NUnit.Framework.Test] public void should_remove_attachment_content_to_plain_storage () =>
            mockOfAttachmentContentPlainStorage.Verify(x => x.Remove(contentHash), Times.Once);

        private static AttachmentContentService attachmentContentService;
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> mockOfAttachmentContentPlainStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
        private static string contentHash = "content hash";
    }
}
