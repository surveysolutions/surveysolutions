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
            attachmentContentPlainStorage.Store(expectedContent, expectedContent.ContentHash);
            attachmentContentService = Create.AttachmentContentService(attachmentContentPlainStorage);
        };

        Because of = () =>
            actualContent = attachmentContentService.GetAttachmentContent(expectedContent.ContentHash);

        It should_return_specified_attachment_content = () =>
        {
            expectedContent.ContentHash.ShouldEqual(actualContent.ContentHash);
            expectedContent.Content.SequenceEqual(actualContent.Content);
        };

        private static AttachmentContentService attachmentContentService;
        private static AttachmentContent actualContent;

        private static readonly AttachmentContent expectedContent = Create.AttachmentContent();
    }
}