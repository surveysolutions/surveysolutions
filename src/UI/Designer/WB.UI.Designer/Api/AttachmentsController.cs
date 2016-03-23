using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var attachment = this.attachmentService.GetAttachmentWithContent(id);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            return CreateAttachmentResponse(attachment, attachment.Content);
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}")]
        public HttpResponseMessage Thumbnail(Guid id)
        {
            var attachment = this.attachmentService.GetAttachmentWithContent(id);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            return CreateAttachmentResponse(attachment, GetTrasformedContent(attachment.Content));
        }

        [HttpGet]
        [Route("thumbnail/{id:Guid}/{size:int}")]
        public HttpResponseMessage Thumbnail(Guid id, int size)
        {
            var attachment = this.attachmentService.GetAttachmentWithContent(id);

            if (attachment == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            return CreateAttachmentResponse(attachment, GetTrasformedContent(attachment.Content, size));
        }

        private static HttpResponseMessage CreateAttachmentResponse(QuestionnaireAttachment attachment, byte[] source)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(source)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = attachment.FileName };
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachment.AttachmentContentId + "\"");

            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = new TimeSpan(2, 0, 0),
                Public = true
            };

            return response;
        }

        private static byte[] GetTrasformedContent(byte[] source, int? sizeToScale = null)
        {
            //later should handle video and produce image preview 

            var defaultSizeToScale = sizeToScale ?? defaultImageSizeToScale;;

            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = defaultSizeToScale,
                    MaxHeight = defaultSizeToScale
                });

                return outputStream.ToArray();
            }
        }
    }
}
