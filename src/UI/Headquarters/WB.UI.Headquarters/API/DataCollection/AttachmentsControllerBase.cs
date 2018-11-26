using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class AttachmentsControllerBase : ApiController
    {
        private readonly IAttachmentContentService attachmentContentService;

        protected AttachmentsControllerBase(IAttachmentContentService attachmentContentService)
        {
            this.attachmentContentService = attachmentContentService;
        }

        public virtual HttpResponseMessage GetAttachmentContent(string id)
        {
            var attachmentContent = this.attachmentContentService.GetAttachmentContent(id);

            if (attachmentContent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachmentContent.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\"");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}
