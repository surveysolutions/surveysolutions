using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using ImageResizer;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
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
            return this.CreateAttachmentResponse(id, defaultImageSizeToScale, true);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}/{size:int}", Name = "AttachmentThumbnailWithSize")]
        public HttpResponseMessage Thumbnail(Guid id, int size)
        {
            return this.CreateAttachmentResponse(id, size, true);
        }

        private HttpResponseMessage CreateAttachmentResponse(Guid attachmentId, int? sizeToScale = null, bool thumbnail = false)
        {
            AttachmentMeta attachment = this.attachmentService.GetAttachmentMeta(attachmentId);

            if (attachment == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            if (this.Request.Headers.IfNoneMatch.Any(x => x.Tag.Trim('"') == attachment.ContentId))
                return this.Request.CreateResponse(HttpStatusCode.NotModified);

            AttachmentContent attachmentContent = this.attachmentService.GetContent(attachment.ContentId);
            
            var content = thumbnail ? GetThumbnailBytes(attachmentContent, sizeToScale) : attachmentContent.Content;

            return Attachment(attachmentContent, attachment.FileName, content);
        }

        private HttpResponseMessage Attachment(AttachmentContent attachmentContent, string filename, byte[] content)
        {
            if (content != null)
            {
                if (attachmentContent.IsVideo() || attachmentContent.IsImage() || (attachmentContent.IsPdf() && attachmentContent.Details?.Thumbnail != null))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(content)
                        {
                            Headers =
                            {
                                ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType),
                                ContentDisposition = new ContentDispositionHeaderValue("attachment"){FileNameStar = filename}
                            }
                        },
                        Headers =
                        {
                            ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\"")
                        }
                    };
                }

                if (attachmentContent.IsAudio() || (attachmentContent.IsPdf() && attachmentContent.Details?.Thumbnail == null))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(content)
                        {
                            Headers = { ContentType = new MediaTypeHeaderValue("image/svg+xml") }
                        }
                    };
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        private byte[] GetThumbnailBytes(AttachmentContent attachmentContent, int? sizeToScale)
        {
            if (attachmentContent.IsVideo())
                return GetTrasformedContent(attachmentContent.Details?.Thumbnail, sizeToScale);
            if (attachmentContent.IsImage())
                return GetTrasformedContent(attachmentContent.Content, sizeToScale);
            if (attachmentContent.IsAudio())
                return File.ReadAllBytes(HostingEnvironment.MapPath(@"~/Content/images/icons-files-audio.svg"));
            if (attachmentContent.IsPdf())
            {
                var thumbnail = attachmentContent.Details?.Thumbnail;

                return thumbnail != null
                    ? GetTrasformedContent(thumbnail, sizeToScale)
                    : File.ReadAllBytes(HostingEnvironment.MapPath(@"~/Content/images/icons-files-pdf.svg"));
            }

            return null;
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
