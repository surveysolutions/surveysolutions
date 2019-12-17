using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [Authorize(Roles="Interviewer")]
    public class AttachmentsApiV2Controller : AttachmentsControllerBase
    {
        public AttachmentsApiV2Controller(IAttachmentContentService attachmentContentService) : base(attachmentContentService)
        {
        }

        [HttpGet]
        [Route("api/interviewer/v2/attachments/{id}")]
        public override IActionResult GetAttachmentContent(string id) => base.GetAttachmentContent(id);
    }
}
