using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    public class AuditLogApiV2Controller : AuditLogControllerBase
    {
        public AuditLogApiV2Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage,
            IPlainStorageAccessor<GlobalAuditLogRecord> globalAuditLogStorage) 
            : base(auditLogStorage, globalAuditLogStorage)
        {
        }

        [HttpPost]
        [Route("api/interviewer/v2/auditlog")]
        public override IActionResult Post([FromBody]AuditLogEntitiesApiView entities) => base.Post(entities);
    }
}
