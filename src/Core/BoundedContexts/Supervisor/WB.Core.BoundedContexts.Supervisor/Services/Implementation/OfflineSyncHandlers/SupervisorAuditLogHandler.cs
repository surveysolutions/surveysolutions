using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorAuditLogHandler : IHandleCommunicationMessage
    {
        private readonly IJsonAllTypesSerializer typesSerializer;
        private readonly IAuditLogService auditLogService;
        private readonly IPlainStorage<UnexpectedExceptionFromInterviewerView, int?> unexpectedExceptionsStorage;

        public SupervisorAuditLogHandler(IAuditLogService auditLogService,
            IJsonAllTypesSerializer typesSerializer, 
            IPlainStorage<UnexpectedExceptionFromInterviewerView, int?> unexpectedExceptionsStorage)
        {
            this.auditLogService = auditLogService;
            this.typesSerializer = typesSerializer;
            this.unexpectedExceptionsStorage = unexpectedExceptionsStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<UploadAuditLogEntityRequest, OkResponse>(UploadAuditLog);
            requestHandler.RegisterHandler<SendUnexpectedExceptionRequest, OkResponse>(SendUnexpectedExceptionHandler);
        }

        public Task<OkResponse> SendUnexpectedExceptionHandler(SendUnexpectedExceptionRequest arg)
        {
            this.unexpectedExceptionsStorage.Store(new UnexpectedExceptionFromInterviewerView
            {
                InterviewerId = arg.UserId,
                StackTrace = arg.Exception.StackTrace,
                Message = arg.Exception.Message
            });
            return OkResponse.Task;
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
