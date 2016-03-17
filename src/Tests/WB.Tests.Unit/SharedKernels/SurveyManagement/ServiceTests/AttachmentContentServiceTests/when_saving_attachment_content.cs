using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_saving_attachment_content : CommentsExporterTestsContext
    {
        Establish context = () =>
        {
            attachmentContentService = Create.CreateAttachmentContentService(mockOfAttachmentContentPlainStorage.Object);
        };

        Because of = () =>
            attachmentContentService.SaveAttachmentContent(contentHash, contentType, content);

        It should_store_attachment_content_to_plain_storage = () =>
            mockOfAttachmentContentPlainStorage.Verify(x=>x.Store(Moq.It.IsAny<AttachmentContent>(), contentHash), Times.Once);

        private static AttachmentContentService attachmentContentService;
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> mockOfAttachmentContentPlainStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
        private static string contentHash = "content hash";
        private static string contentType = "image/png";
        private static readonly byte[] content = new byte[] {1, 2, 3};
    }
}