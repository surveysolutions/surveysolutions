using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorAuditLogHandler : IHandleCommunicationMessage
    {
        private readonly IJsonAllTypesSerializer typesSerializer;
        private readonly IAuditLogService auditLogService;

        public SupervisorAuditLogHandler(IAuditLogService auditLogService, IJsonAllTypesSerializer typesSerializer)
        {
            this.auditLogService = auditLogService;
            this.typesSerializer = typesSerializer;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<UploadAuditLogEntityRequest, OkResponse>(UploadAuditLog);
        }

        static SupervisorAuditLogHandler()
        {
            var audit = typeof(IAuditLogEntity);

            AuditLogTypeMap = Assembly.GetAssembly(typeof(IAuditLogEntity))
                .GetTypes()
                .Where(t => audit.IsAssignableFrom(t)).ToDictionary(t => t.Name);
        }

        private static readonly Dictionary<string, Type> AuditLogTypeMap;

        public Task<OkResponse> UploadAuditLog(UploadAuditLogEntityRequest request)
        {
            foreach (var entity in request.AuditLogEntity.Entities)
            {
                var type = AuditLogTypeMap[entity.PayloadType];

                auditLogService.WriteAuditLogRecord(new AuditLogEntityView
                {
                    Type = entity.Type,
                    Payload = typesSerializer.Deserialize<IAuditLogEntity>(entity.Payload, type),
                    ResponsibleId = entity.ResponsibleId,
                    ResponsibleName = entity.ResponsibleName,
                    Time = entity.Time.DateTime,
                    TimeUtc = entity.TimeUtc.DateTime
                });
            }

            return Task.FromResult(new OkResponse());
        }
    }
}
