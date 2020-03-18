using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class AuditLogControllerBase : ControllerBase
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> auditLogStorage;

        protected AuditLogControllerBase(IPlainStorageAccessor<AuditLogRecord> auditLogStorage)
        {
            this.auditLogStorage = auditLogStorage;
        }

        public virtual IActionResult Post(AuditLogEntitiesApiView entities)
        {
            if (entities == null)
                return this.BadRequest("Server cannot accept empty package content.");

            foreach (var auditLogEntity in entities.Entities)
            {
                var auditLogRecord = new AuditLogRecord()
                {
                    RecordId = auditLogEntity.Id,
                    ResponsibleId = auditLogEntity.ResponsibleId,
                    ResponsibleName = auditLogEntity.ResponsibleName,
                    Time = auditLogEntity.Time.DateTime,
                    TimeUtc = auditLogEntity.TimeUtc.DateTime,
                    Type = auditLogEntity.Type,
                };
                auditLogRecord.SetJsonPayload(auditLogEntity.Payload);
                auditLogStorage.Store(auditLogRecord, auditLogRecord.Id);
            }

            return this.Ok();
        }
    }
}
