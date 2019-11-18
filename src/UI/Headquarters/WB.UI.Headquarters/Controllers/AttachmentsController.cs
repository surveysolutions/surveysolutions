using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Shared.Web.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class AttachmentsController : ApiController
    {
        private readonly IAttachmentContentService attachmentContentService;
        private readonly IImageProcessingService imageProcessingService;

        public AttachmentsController(IAttachmentContentService attachmentContentService, IImageProcessingService imageProcessingService)
        {
            this.attachmentContentService = attachmentContentService;
            this.imageProcessingService = imageProcessingService;
        }
        
        [HttpGet]
        public HttpResponseMessage Content(string id, int? maxSize = null)
        {
            if (this.Request.Headers.IfNoneMatch.Any(x => x.Tag.Trim('"') == id))
                return this.Request.CreateResponse(HttpStatusCode.NotModified);

            var attachmentContent = this.attachmentContentService.GetAttachmentContent(id);

            if(attachmentContent == null)
                return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(GetTrasformedContent(attachmentContent.Content, maxSize))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\"");
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.MaxValue,
                Public = true
            };

            return response;
        }

        private byte[] GetTrasformedContent(byte[] source, int? sizeToScale = null)
        {
            if (!sizeToScale.HasValue) return source;

            return this.imageProcessingService.ResizeImage(source, sizeToScale.Value, sizeToScale.Value);
        }
    }
}
