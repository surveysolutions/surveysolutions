using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class AttachmentsControllerBase : ControllerBase
    {
        private readonly IAttachmentContentService attachmentContentService;

        protected AttachmentsControllerBase(IAttachmentContentService attachmentContentService)
        {
            this.attachmentContentService = attachmentContentService;
        }

        public virtual IActionResult GetAttachmentContent(string id)
        {
            var attachmentContent = this.attachmentContentService.GetAttachmentContent(id);

            if (attachmentContent == null)
                return NotFound();

            var result = File(attachmentContent.Content, attachmentContent.ContentType, null,
                new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\""));

            return result;
        }
    }
}
