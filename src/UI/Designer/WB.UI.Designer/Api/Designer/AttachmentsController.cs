using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
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
            return this.CreateAttachmentResponse(id);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}", Name = "AttachmentThumbnail")]
        public HttpResponseMessage Thumbnail(Guid id)
        {
            return this.CreateAttachmentResponse(id, defaultImageSizeToScale);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}/{size:int}", Name = "AttachmentThumbnailWithSize")]
        public HttpResponseMessage Thumbnail(Guid id, int size)
        {
            return this.CreateAttachmentResponse(id, size);
        }

        private HttpResponseMessage CreateAttachmentResponse(Guid attachmentId, int? sizeToScale = null)
        {
            var attachment = this.attachmentService.GetAttachmentMeta(attachmentId);

            if (attachment == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            if (this.Request.Headers.IfNoneMatch.Any(x => x.Tag.Trim('"') == attachment.ContentId))
                return this.Request.CreateResponse(HttpStatusCode.NotModified);

            var attachmentContent = this.attachmentService.GetContent(attachment.ContentId);

            if (attachmentContent.IsImage())
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(GetTrasformedContent(attachmentContent.Content, sizeToScale))
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileNameStar = attachment.FileName};
                response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\"");
                return response;
            }

            string imagePath = null;
            if (attachmentContent.IsAudio())
            {
                imagePath = HostingEnvironment.MapPath("~/Content/images/icons-files-audio.svg");
            }
            if (attachmentContent.IsPdf())
            {
                imagePath = HostingEnvironment.MapPath("~/Content/images/icons-files-pdf.svg");
            }

            if (imagePath != null)
            {
                var byteArrayContent = new ByteArrayContent(File.ReadAllBytes(imagePath));
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");

                var httpResponseMessage = new HttpResponseMessage
                {
                    Content = byteArrayContent
                };

                return httpResponseMessage;
            }

            return new HttpResponseMessage(HttpStatusCode.NoContent);
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
