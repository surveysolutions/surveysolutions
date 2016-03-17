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
            attachmentContentPlainStorage.Store(expectedContent, contentHash);
            attachmentContentService = Create.CreateAttachmentContentService(attachmentContentPlainStorage);
        };

        Because of = () =>
            actualContent = attachmentContentService.GetAttachmentContent(contentHash);

        It should_return_specified_attachment_content = () =>
        {
            expectedContent.ContentHash.ShouldEqual(actualContent.ContentHash);
            expectedContent.Content.SequenceEqual(actualContent.Content);
        };

        private static AttachmentContentService attachmentContentService;
        private static string contentHash = "content id";
        private static AttachmentContent actualContent;

        private static readonly AttachmentContent expectedContent = new AttachmentContent
        {
            ContentHash = "content id",
            Content = new byte[] {1, 2, 3}
        };
    }
}