using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Code.Attributes;

namespace WB.UI.Designer.Api.Headquarters
{
    [Route("api/hq/attachment")]
    public class HQAttachmentController : ControllerBase
    {
        private readonly IAttachmentService attachmentService;
        public HQAttachmentController(IAttachmentService attachmentService)
        {
            this.attachmentService = attachmentService;
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(string id, [FromQuery] Guid? attachmentId)
        {
            var attachment = this.attachmentService.GetContent(id);

            if (attachment == null) return NotFound();

            string fileName = null;
            if (attachmentId.HasValue)
            {
                var attachmentMeta = this.attachmentService.GetAttachmentMeta(attachmentId.Value);
                fileName = attachmentMeta.FileName;
            }

            return File(attachment.Content, attachment.ContentType, fileName, null,
                new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + attachment.ContentId + "\""));
        }
    }
}
