using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class AttachmentsController : ApiController
    {
        private readonly IAttachmentContentService attachmentContentService;

        public AttachmentsController(IAttachmentContentService attachmentContentService)
        {
            this.attachmentContentService = attachmentContentService;
        }
        
        [HttpGet]
        public HttpResponseMessage Content(string id)
        {
            var attachment = this.attachmentContentService.GetAttachmentContent(id);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachment.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.ContentHash + "\"");

            return response;
        }
    }
}