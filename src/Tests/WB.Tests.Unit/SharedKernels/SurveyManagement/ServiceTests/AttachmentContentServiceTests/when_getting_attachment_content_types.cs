using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_getting_attachment_content_types
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(expectedContent, contentHash);
            attachmentContentService = Create.Service.AttachmentContentService(attachmentContentPlainStorage);
            BecauseOf();
        }

        public void BecauseOf() =>
            result = attachmentContentService.GetAttachmentInfosByContentIds(new List<string>() {  contentHash});

        [NUnit.Framework.Test] public void should_return_one_item () =>
            result.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_correct_value () 
        {
            result.First().ContentHash.Should().Be(contentHash);
            result.First().ContentType.Should().Be(contentType);
        }

        private static AttachmentContentService attachmentContentService;
        private static string contentHash = "content id";
        private static string contentType = "content type";

        private static IEnumerable<AttachmentInfoView> result;

        private static readonly AttachmentContent expectedContent = new AttachmentContent
        {
            ContentHash = contentHash,
            ContentType = contentType,
            Content = new byte[] {1, 2, 3}
        };
    }
}
