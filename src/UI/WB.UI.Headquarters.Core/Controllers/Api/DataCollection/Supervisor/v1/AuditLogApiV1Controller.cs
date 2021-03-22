using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class AuditLogApiV1Controller : AuditLogControllerBase
    {
        public AuditLogApiV1Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage,
            IPlainStorageAccessor<GlobalAuditLogRecord> globalAuditLogStorage,
            IInScopeExecutor scopeExecutor,
            IWorkspacesCache workspacesCache,
            ILogger<AuditLogApiV1Controller> logger) 
            : base(auditLogStorage, globalAuditLogStorage, scopeExecutor, workspacesCache, logger)
        {
        }

        [HttpPost]
        [Route("api/supervisor/v1/auditlog")]
        public override IActionResult Post([FromBody]AuditLogEntitiesApiView entities) => base.Post(entities);
    }
}
