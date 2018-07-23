using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_getting_attachment_content : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            attachmentContent = Create.Entity.AttachmentContent_SurveyManagement(
                contentHash: attachmentContentId,
                contentType: "image/png",
                content: new byte[]{ 1,2,3 }
            );

            attachmentContentService = Mock.Of<IAttachmentContentService>(
                x => x.GetAttachmentContent(attachmentContentId) == attachmentContent);

            controller = CreateAttachmentsApiV2Controller(attachmentContentService);
            BecauseOf();
        }
        
        public void BecauseOf() 
        {
            responseMessage = controller.GetAttachmentContent(attachmentContentId);
        }

        [NUnit.Framework.Test] public void should_return_HttpResponseMessage () =>
            responseMessage.Should().BeOfType<HttpResponseMessage>();

        [NUnit.Framework.Test] public void should_call_service_get_once () =>
            Mock.Get(attachmentContentService).Verify(x => x.GetAttachmentContent(attachmentContentId), Times.Once());

        [NUnit.Framework.Test] public void should_return_content_type_in_header () => 
            responseMessage.Content.Headers.ContentType.MediaType.Should().Be(attachmentContent.ContentType);

        [NUnit.Framework.Test] public void should_return_content_hash_in_header () => 
            responseMessage.Headers.ETag.Tag.Should().Be("\"" + attachmentContent.ContentHash + "\"");

        [NUnit.Framework.Test] public async Task should_return_content_in_body ()
        {
            var result = await responseMessage.Content.ReadAsByteArrayAsync();
            result.Should().BeEquivalentTo(attachmentContent.Content);
        }


        private static string attachmentContentId = "11111111111111111111111111111111";
        private static AttachmentContent attachmentContent; 
        private static HttpResponseMessage responseMessage;
        private static AttachmentsApiV2Controller controller;
        private static IAttachmentContentService attachmentContentService;
    }
}
