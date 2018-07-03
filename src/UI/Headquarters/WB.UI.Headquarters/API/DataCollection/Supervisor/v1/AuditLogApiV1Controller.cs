using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class AuditLogApiV1Controller : AuditLogControllerBase
    {
        public AuditLogApiV1Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage) : base(auditLogStorage)
        {
        }

        [HttpPost]
        public override IHttpActionResult Post(AuditLogEntitiesApiView entities) => base.Post(entities);
    }
}
