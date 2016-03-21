using System;
using System.Net.Http;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_getting_attachment_content : ApiTestContext
    {
        private Establish context = () =>
        {
            attachmentContent = Create.AttachmentContent(
                contentHash: attachmentContentId,
                contentType: "image/png",
                content: new byte[]{ 1,2,3 }
            );

            attachmentContentService = Mock.Of<IAttachmentContentService>(
                x => x.GetAttachmentContent(attachmentContentId) == attachmentContent);

            controller = CreateAttachmentsApiV2Controller(attachmentContentService);
        };
        
        Because of = () =>
        {
            responseMessage = controller.GetAttachmentContent(attachmentContentId);
        };

        It should_return_HttpResponseMessage = () =>
            responseMessage.ShouldBeOfExactType<HttpResponseMessage>();

        It should_call_service_get_once = () =>
            Mock.Get(attachmentContentService).Verify(x => x.GetAttachmentContent(attachmentContentId), Times.Once());

        It should_return_content_type_in_header = () => 
            responseMessage.Content.Headers.ContentType.MediaType.ShouldEqual(attachmentContent.ContentType);

        It should_return_content_hash_in_header = () => 
            responseMessage.Headers.ETag.Tag.ShouldEqual("\"" + attachmentContent.ContentHash + "\"");

        It should_return_content_in_body = () => 
            responseMessage.Content.ReadAsByteArrayAsync().Result.ShouldEqual(attachmentContent.Content);



        private static string attachmentContentId = "11111111111111111111111111111111";
        private static AttachmentContent attachmentContent; 
        private static HttpResponseMessage responseMessage;
        private static AttachmentsApiV2Controller controller;
        private static IAttachmentContentService attachmentContentService;
    }
}
