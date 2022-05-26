using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [Route("api/v{version:int}/attachment")]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService attachmentService;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public AttachmentController(IAttachmentService attachmentService, IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.attachmentService = attachmentService;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [Authorize]        
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(string id, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode((int) HttpStatusCode.UpgradeRequired);

            var attachmentContent = this.attachmentService.GetContent(id);

            if (attachmentContent?.Content == null) return NotFound();

            return File(attachmentContent.Content, attachmentContent.ContentType,
                null,
                new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\""));
        }
        
        [AuthorizeOrAnonymousQuestionnaire]
        [QuestionnairePermissions]
        [HttpGet]
        [Route("{id}/{attachmentContentId}")]
        public IActionResult Get(QuestionnaireRevision id, string attachmentContentId, int version)
        {
            if (version < ApiVersion.CurrentTesterProtocolVersion)
                return StatusCode((int) HttpStatusCode.UpgradeRequired);

            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();

            var exists = questionnaireView.Source.Attachments.Any(a => a.ContentId == attachmentContentId);
            if (!exists)
                return NotFound();

            var attachmentContent = this.attachmentService.GetContent(attachmentContentId);
            if (attachmentContent?.Content == null) 
                return NotFound();

            return File(attachmentContent.Content, attachmentContent.ContentType,
                null,
                new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\""));
        }
    }
}
