using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_getting_attachment_content : CommentsExporterTestsContext
    {
        Establish context = () =>
        {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(
                new AttachmentContent
                {
                    ContentHash = contentHash,
                    Content = expectedContent
                }, contentHash);
            attachmentContentService = Create.CreateAttachmentContentService(attachmentContentPlainStorage);
        };

        Because of = () =>
            actualContent = attachmentContentService.GetAttachmentContent(contentHash);

        It should_return_array_of_bytes_by_content_id_from_plain_storage = () =>
            expectedContent.SequenceEqual(actualContent);

        private static AttachmentContentService attachmentContentService;
        private static string contentHash = "content id";
        private static byte[] actualContent;
        private static readonly byte[] expectedContent = new byte[] {1, 2, 3};
    }
}