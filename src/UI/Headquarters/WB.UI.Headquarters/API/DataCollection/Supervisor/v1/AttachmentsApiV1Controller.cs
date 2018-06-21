using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class AttachmentsApiV1Controller : AttachmentsControllerBase
    {
        public AttachmentsApiV1Controller(IAttachmentContentService attachmentContentService) : base(attachmentContentService)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        public override HttpResponseMessage GetAttachmentContent(string id) => base.GetAttachmentContent(id);
    }

}
