using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ImageResizer;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Api
{
    [RoutePrefix("attachments")]
    public class AttachmentsController : ApiController
    {
        private const int defaultImageSizeToScale = 156;

        private readonly IAttachmentService attachmentService;
        public AttachmentsController(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public HttpResponseMessage Get(Guid id)
        {
            return CreateAttachmentResponse(id);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}")]
        public HttpResponseMessage Thumbnail(Guid id)
        {
            return CreateAttachmentResponse(id, defaultImageSizeToScale);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}/{size:int}")]
        public HttpResponseMessage Thumbnail(Guid id, int size)
        {
            return CreateAttachmentResponse(id, size);
        }

        private HttpResponseMessage CreateAttachmentResponse(Guid attachmentId, int? sizeToScale = null)
        {
            var attachment = this.attachmentService.GetAttachmentMeta(attachmentId);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (this.Request.Headers.IfNoneMatch.Any(x => x.Tag == attachment.ContentId))
                return this.Request.CreateResponse(HttpStatusCode.NotModified);

            var attachmentContent = this.attachmentService.GetContent(attachment.ContentId);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(GetTrasformedContent(attachmentContent.Content, sizeToScale))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = attachment.FileName };
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\"");
            
            return response;
        }

        private static byte[] GetTrasformedContent(byte[] source, int? sizeToScale = null)
        {
            if (!sizeToScale.HasValue) return source;

            //later should handle video and produce image preview 
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = sizeToScale.Value,
                    MaxHeight = sizeToScale.Value
                });

                return outputStream.ToArray();
            }
        }
    }
}
