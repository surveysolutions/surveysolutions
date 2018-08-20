using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public class AuditLogSynchronizer : IAuditLogSynchronizer
    {
        private readonly IAuditLogService auditLogService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IJsonAllTypesSerializer typesSerializer;

        public AuditLogSynchronizer(IAuditLogService auditLogService, ISynchronizationService synchronizationService,
            IJsonAllTypesSerializer typesSerializer)
        {
            this.auditLogService = auditLogService;
            this.synchronizationService = synchronizationService;
            this.typesSerializer = typesSerializer;
        }

        public async Task SynchronizeAuditLogAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_UploadAuditLog,
                Stage = SyncStage.UploadingAuditLog
            });

            auditLogService.Write(new SynchronizationCompletedAuditLogEntity(
                statistics.NewAssignmentsCount,
                statistics.RemovedAssignmentsCount,
                statistics.NewInterviewsCount,
                statistics.SuccessfullyUploadedInterviewsCount,
                statistics.RejectedInterviewsCount,
                statistics.DeletedInterviewsCount
            ));

            var auditLogEntities = auditLogService.GetAuditLogEntitiesForSync();

            AuditLogEntitiesApiView entities = new AuditLogEntitiesApiView();

            entities.Entities = auditLogEntities.Select(auditLogEntity => new AuditLogEntityApiView()
                {
                    Id = auditLogEntity.Id,
                    ResponsibleId = auditLogEntity.ResponsibleId,
                    ResponsibleName = auditLogEntity.ResponsibleName,
                    Time = auditLogEntity.Time,
                    TimeUtc = auditLogEntity.TimeUtc,
                    PayloadType = auditLogEntity.Payload.GetType().Name,
                    Payload = typesSerializer.Serialize(auditLogEntity.Payload),
                    Type = auditLogEntity.Type
                }
            ).ToArray();

            await this.synchronizationService.UploadAuditLogEntityAsync(entities, cancellationToken);
            auditLogService.UpdateLastSyncIndex(entities.Entities.Last().Id);
        }
    }
}
