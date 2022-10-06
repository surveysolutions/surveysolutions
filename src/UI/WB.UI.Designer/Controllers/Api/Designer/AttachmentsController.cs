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
using WB.UI.Designer.Services.AttachmentPreview;

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
        private readonly IAttachmentPreviewHelper attachmentPreviewHelper;

        public AttachmentsController(IAttachmentService attachmentService,
            IWebHostEnvironment environment,
            IDesignerQuestionnaireStorage questionnaireStorage,
            IAttachmentPreviewHelper attachmentPreviewHelper)
        {
            this.attachmentService = attachmentService;
            this.environment = environment;
            this.questionnaireStorage = questionnaireStorage;
            this.attachmentPreviewHelper = attachmentPreviewHelper;
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

            if (attachmentContent.Content == null) return NoContent();

            var previewContent = attachmentPreviewHelper.GetPreviewImage(attachmentContent, sizeToScale);

            if (previewContent == null)
                return NoContent();
            
            return File(previewContent.Content, previewContent.ContentType, attachment.FileName,
                attachment.LastUpdateDate, new EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\""), false);
        }
    }
}
