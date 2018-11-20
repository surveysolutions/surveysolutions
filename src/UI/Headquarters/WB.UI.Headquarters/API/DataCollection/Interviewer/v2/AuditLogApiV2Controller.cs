using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class AuditLogApiV2Controller : AuditLogControllerBase
    {
        public AuditLogApiV2Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage) : base(auditLogStorage)
        {
        }

        [HttpPost]
        public override IHttpActionResult Post(AuditLogEntitiesApiView entities) => base.Post(entities);
    }
}
