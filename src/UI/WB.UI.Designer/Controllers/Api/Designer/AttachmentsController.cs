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
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Route("attachments")]
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    public class AttachmentsController : Controller
    {
        private const int defaultImageSizeToScale = 156;

        private readonly IAttachmentService attachmentService;
        private readonly IWebHostEnvironment environment;
        private readonly IDesignerQuestionnaireStorage questionnaireStorage;

        public AttachmentsController(IAttachmentService attachmentService,
            IWebHostEnvironment environment,
            IDesignerQuestionnaireStorage questionnaireStorage)
        {
            this.attachmentService = attachmentService;
            this.environment = environment;
            this.questionnaireStorage = questionnaireStorage;
        }

        [HttpGet]
        [Route("{id}/{attachmentId:Guid}")]
        public IActionResult Get(QuestionnaireRevision id, Guid attachmentId)
        {
            return this.CreateAttachmentResponse(id, attachmentId);
        }

        [HttpGet]
        [Route("{id}/thumbnail/{attachmentId:Guid}")]
        public IActionResult Thumbnail(QuestionnaireRevision id, Guid attachmentId)
        {
            return this.CreateAttachmentResponse(id, attachmentId, defaultImageSizeToScale, true);
        }

        [HttpGet]
        [Route("{id}/thumbnail/{attachmentId:Guid}/{size:int}")]
        public IActionResult Thumbnail(QuestionnaireRevision id, Guid attachmentId, int size)
        {
            return this.CreateAttachmentResponse(id, attachmentId, size, true);
        }
        
        [HttpGet]
        [Obsolete]
        [AllowAnonymous]
        [Route("thumbnail/{attachmentId:Guid}", Name = "AttachmentThumbnail")]
        public IActionResult Thumbnail(Guid attachmentId)
        {
            return this.CreateAttachmentResponse(null, attachmentId, defaultImageSizeToScale, true);
        }

        [HttpGet]
        [Obsolete]
        [AllowAnonymous]
        [Route("thumbnail/{attachmentId:Guid}/{size:int}", Name = "AttachmentThumbnailWithSize")]
        public IActionResult Thumbnail(Guid attachmentId, int size)
        {
            return this.CreateAttachmentResponse(null, attachmentId, size, true);
        }

        private IActionResult CreateAttachmentResponse(QuestionnaireRevision? questionnaireRevision, Guid attachmentId, int? sizeToScale = null, bool thumbnail = false)
        {
            if (questionnaireRevision != null)
            {
                var questionnaire = questionnaireStorage.Get(questionnaireRevision);
                if (questionnaire == null) return NotFound();

                var hasAttachment = questionnaire.Attachments.Any(a => a.AttachmentId == attachmentId);
                if (!hasAttachment) return NotFound();
            }

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
