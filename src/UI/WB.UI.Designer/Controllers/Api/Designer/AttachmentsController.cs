using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Route("attachments")]
    [Authorize]
    public class AttachmentsController : Controller
    {
        private const int defaultImageSizeToScale = 156;

        private readonly IAttachmentService attachmentService;
        private readonly IWebHostEnvironment environment;

        public AttachmentsController(IAttachmentService attachmentService,
            IWebHostEnvironment environment)
        {
            this.attachmentService = attachmentService;
            this.environment = environment;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public IActionResult Get(Guid id)
        {
            return this.CreateAttachmentResponse(id);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("thumbnail/{id:Guid}", Name = "AttachmentThumbnail")]
        public IActionResult Thumbnail(Guid id)
        {
            return this.CreateAttachmentResponse(id, defaultImageSizeToScale, true);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("thumbnail/{id:Guid}/{size:int}", Name = "AttachmentThumbnailWithSize")]
        public IActionResult Thumbnail(Guid id, int size)
        {
            return this.CreateAttachmentResponse(id, size, true);
        }

        private IActionResult CreateAttachmentResponse(Guid attachmentId, int? sizeToScale = null, bool thumbnail = false)
        {
            AttachmentMeta? attachment = this.attachmentService.GetAttachmentMeta(attachmentId);

            if (attachment == null) return NotFound();

            var requestHeaders = new RequestHeaders(this.Request.Headers);

            if (requestHeaders.IfNoneMatch?.Any(x => x.Tag.ToString().Trim('"') == attachment.ContentId) ?? false)
                return base.StatusCode((int)HttpStatusCode.NotModified);

            AttachmentContent? attachmentContent = this.attachmentService.GetContent(attachment.ContentId);
            if (attachmentContent == null) return NotFound();

            if (thumbnail)
            {
                string contentType = "image/jpg";
                byte[]? thumbBytes = null;
                if (attachmentContent.Details.Thumbnail == null || attachmentContent.Details.Thumbnail.Length == 0)
                {
                    if (attachmentContent.IsImage())
                    {
                        thumbBytes = attachmentContent.Content;
                    }

                    if (attachmentContent.IsAudio())
                    {
                        thumbBytes = System.IO.File.ReadAllBytes(environment.MapPath("images/icons-files-audio.png"));
                        contentType = @"image/png";
                    }

                    if (attachmentContent.IsPdf())
                    {
                        thumbBytes = System.IO.File.ReadAllBytes(environment.MapPath(@"images/icons-files-pdf.png"));
                        contentType = @"image/png";
                    }
                }
                else
                {
                    thumbBytes = attachmentContent.Details.Thumbnail;
                }

                if (thumbBytes == null)
                {
                    return NoContent();
                }

                if (sizeToScale != null && contentType == "image/jpg")
                {
                    thumbBytes = GetTransformedContent(thumbBytes, sizeToScale);
                }

                return File(thumbBytes, contentType);
            }

            if (attachmentContent.Content == null) return NoContent();

            return File(attachmentContent.Content, attachmentContent.ContentType, attachment.FileName,
                attachment.LastUpdateDate, new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\""), false);
        }

        private static byte[] GetTransformedContent(byte[] source, int? sizeToScale = null)
        {
            if (!sizeToScale.HasValue) return source;
            using (var outputStream = new MemoryStream())
            {
                using (Image image = Image.Load(source, out var format))
                {
                    var opt = new ResizeOptions()
                    {
                       Mode = ResizeMode.Max,
                       Size = new Size(sizeToScale.Value)
                    };
                    image.Mutate(ctx => ctx.Resize(opt)); 
                    image.Save(outputStream, format); 
                } 

                return outputStream.ToArray();
            }
        }
    }
}
