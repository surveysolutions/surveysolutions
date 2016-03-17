using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_getting_attachment_content_types
    {
        Establish context = () =>
        {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(expectedContent, contentHash);
            attachmentContentService = Create.CreateAttachmentContentService(attachmentContentPlainStorage);
        };

        Because of = () =>
            result = attachmentContentService.GetContentTypes(new HashSet<string>() { { contentHash} });

        It should_return_one_item = () =>
            result.Count.ShouldEqual(1);

        It should_return_correct_key = () =>
            result.First().Key.ShouldEqual(contentHash);

        It should_return_correct_value = () =>
        {
            result.First().Value.ContentHash.ShouldEqual(contentHash);
            result.First().Value.ContentType.ShouldEqual(contentType);
        };

        private static AttachmentContentService attachmentContentService;
        private static string contentHash = "content id";
        private static string contentType = "content type";

        private static Dictionary<string, AttachmentInfoView> result;

        private static readonly AttachmentContent expectedContent = new AttachmentContent
        {
            ContentHash = contentHash,
            ContentType = contentType,
            Content = new byte[] {1, 2, 3}
        };
    }
}