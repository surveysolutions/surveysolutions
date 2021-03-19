using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class AuditLogApiV1Controller : AuditLogControllerBase
    {
        public AuditLogApiV1Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage,
            IPlainStorageAccessor<GlobalAuditLogRecord> globalAuditLogStorage) 
            : base(auditLogStorage, globalAuditLogStorage)
        {
        }

        [HttpPost]
        [Route("api/supervisor/v1/auditlog")]
        public override IActionResult Post([FromBody]AuditLogEntitiesApiView entities) => base.Post(entities);
    }
}
