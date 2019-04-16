using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Controllers.Api.Tester
{
    [ApiBasicAuth]
    [Route("api/attachment")]
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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired));

            var attachmentContent = this.attachmentService.GetContent(id);

            if (attachmentContent == null) return NotFound();

            return File(attachmentContent.Content, attachmentContent.ContentType,
                null,
                new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + attachmentContent.ContentId + "\""));
        }
    }
}
