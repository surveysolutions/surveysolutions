using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Shared.Web.Extensions;

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

            var result = this.AsProgressiveDownload(attachmentContent.Content, attachmentContent.ContentType);

            if (result.StatusCode != HttpStatusCode.PartialContent)
            {
                result.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\"");
                result.Headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromDays(10)
                };
            }

            return result;
        }
    }
}
