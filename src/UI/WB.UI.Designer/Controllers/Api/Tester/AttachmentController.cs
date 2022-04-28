using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [AuthorizeOrAnonymousQuestionnaire]
    [Route("api/v{version:int}/attachment")]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService attachmentService;
        public AttachmentController(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }

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
    }
}
