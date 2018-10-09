using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_check_existing_attachment_content 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(expectedContent, expectedContent.ContentHash);
            attachmentContentService = Create.Service.AttachmentContentService(attachmentContentPlainStorage);
            BecauseOf();
        }

        public void BecauseOf() =>
            isAttachmentContentExists = attachmentContentService.HasAttachmentContent(expectedContent.ContentHash);

        [NUnit.Framework.Test] public void should_attachment_content_exists_in_plain_storage () =>
            isAttachmentContentExists.Should().BeTrue();

        private static AttachmentContentService attachmentContentService;
        private static readonly AttachmentContent expectedContent = Create.Entity.AttachmentContent_SurveyManagement();
        private static bool isAttachmentContentExists;
    }
}
