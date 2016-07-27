using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Headquarters
{
    [ApiBasicAuth]
    [RoutePrefix("api/hq/attachment")]
    public class HQAttachmentController : ApiController
    {
        private readonly IAttachmentService attachmentService;
        public HQAttachmentController(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(string id)
        {
            var attachment = this.attachmentService.GetContent(id);

            if (attachment == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachment.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.ContentId + "\"");

            return response;
        }
    }
}