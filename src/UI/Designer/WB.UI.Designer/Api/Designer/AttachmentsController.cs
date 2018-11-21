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

            HttpResponseMessage CreateResponse(byte[] data, string contentType)
            {
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(data)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue(contentType)
                        }
                    }
                };

                return message;
            }

            if (thumbnail)
            {
                string contentType = "image/jpg";
                byte[] thumbBytes = null;
                if (attachmentContent.Details.Thumbnail == null)
                {
                    if (attachmentContent.IsImage())
                    {
                        thumbBytes = attachmentContent.Content;
                    }

                    if (attachmentContent.IsAudio())
                    {
                        thumbBytes = File.ReadAllBytes(HostingEnvironment.MapPath(@"~/Content/images/icons-files-audio.png"));
                        contentType = @"image/png";
                    }

                    if (attachmentContent.IsPdf())
                    {
                        thumbBytes = File.ReadAllBytes(HostingEnvironment.MapPath(@"~/Content/images/icons-files-pdf.png"));
                        contentType = @"image/png";
                    }
                }
                else
                {
                    thumbBytes = attachmentContent.Details.Thumbnail;
                }

                if (thumbBytes == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                if (sizeToScale != null && contentType == "image/jpg")
                {
                    thumbBytes = GetTransformedContent(thumbBytes, sizeToScale);
                }

                return CreateResponse(thumbBytes, contentType);
            }

            if (attachmentContent.Content == null) return new HttpResponseMessage(HttpStatusCode.NoContent);

            var response = CreateResponse(attachmentContent.Content, attachmentContent.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = attachment.FileName
            };
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\"");
            return response;
        }

        private static byte[] GetTransformedContent(byte[] source, int? sizeToScale = null)
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
