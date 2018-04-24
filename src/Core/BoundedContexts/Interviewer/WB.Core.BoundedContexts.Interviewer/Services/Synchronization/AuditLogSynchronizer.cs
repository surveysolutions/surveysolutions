using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class AuditLogSynchronizer : IAuditLogSynchronizer
    {
        private readonly IAuditLogService auditLogService;
        private readonly ISynchronizationService synchronizationService;

        public AuditLogSynchronizer(IAuditLogService auditLogService, ISynchronizationService synchronizationService)
        {
            this.auditLogService = auditLogService;
            this.synchronizationService = synchronizationService;
        }

        public async Task SynchronizeAuditLogAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_DownloadingLogo
            });

            var auditLogEntities = auditLogService.GetAuditLogEntitiesForSync();

            foreach (var auditLogEntity in auditLogEntities)
            {
                AuditLogEntityApiView auditLogEntityView = new AuditLogEntityApiView()
                {
                    Id = auditLogEntity.Id,
                    ResponsibleId = auditLogEntity.ResponsibleId,
                    Time = auditLogEntity.Time,
                    TimeUtc = auditLogEntity.TimeUtc,
                    Payload = auditLogEntity.Payload,
                    Type = auditLogEntity.Type
                };
                await this.synchronizationService.UploadAuditLogEntityAsync(auditLogEntityView, cancellationToken);
                auditLogService.UpdateLastSyncIndex(auditLogEntity.Id);
            }
        }
    }
}
