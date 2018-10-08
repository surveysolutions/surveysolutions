using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AttachmentContentServiceTests
{
    class when_getting_attachment_content 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var attachmentContentPlainStorage = new TestPlainStorage<AttachmentContent>();
            attachmentContentPlainStorage.Store(expectedContent, expectedContent.ContentHash);
            attachmentContentService = Create.Service.AttachmentContentService(attachmentContentPlainStorage);
            BecauseOf();
        }

        public void BecauseOf() =>
            actualContent = attachmentContentService.GetAttachmentContent(expectedContent.ContentHash);

        [NUnit.Framework.Test] public void should_return_specified_attachment_content () 
        {
            expectedContent.ContentHash.Should().Be(actualContent.ContentHash);
            expectedContent.Content.SequenceEqual(actualContent.Content);
        }

        private static AttachmentContentService attachmentContentService;
        private static AttachmentContent actualContent;

        private static readonly AttachmentContent expectedContent = Create.Entity.AttachmentContent_SurveyManagement();
    }
}
