using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class AttachmentsApiV1Controller : AttachmentsControllerBase
    {
        public AttachmentsApiV1Controller(IAttachmentContentService attachmentContentService) : base(attachmentContentService)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        [Route("api/supervisor/v1/attachments/{id}")]
        public override IActionResult GetAttachmentContent(string id) => base.GetAttachmentContent(id);
    }

}
