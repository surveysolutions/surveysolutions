using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;


namespace WB.UI.Headquarters.API.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class AuditLogApiV2Controller : ApiController
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> auditLogStorage;

        public AuditLogApiV2Controller(IPlainStorageAccessor<AuditLogRecord> auditLogStorage)
        {
            this.auditLogStorage = auditLogStorage;
        }

        [HttpPost]
        public IHttpActionResult Post(AuditLogEntityApiView auditLogEntity)
        {
            if (auditLogEntity == null)
                return this.BadRequest("Server cannot accept empty package content.");

            var auditLogRecord = new AuditLogRecord()
            {
                RecordId = auditLogEntity.Id,
                ResponsibleId = auditLogEntity.ResponsibleId,
                ResponsibleName = auditLogEntity.ResponsibleName,
                Time = auditLogEntity.Time,
                TimeUtc = auditLogEntity.TimeUtc,
                Type = auditLogEntity.Type,
                Payload = auditLogEntity.Payload
            };
            auditLogStorage.Store(auditLogRecord, auditLogRecord.Id);

            return this.Ok();
        }
    }
}
