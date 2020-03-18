using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles="Interviewer")]
    public class AttachmentsApiV2Controller : AttachmentsControllerBase
    {
        public AttachmentsApiV2Controller(IAttachmentContentService attachmentContentService) : base(attachmentContentService)
        {
        }

        [HttpGet]
        [Route("api/interviewer/v2/attachments/{id}")]
        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        public override IActionResult GetAttachmentContent(string id) => base.GetAttachmentContent(id);
    }
}
