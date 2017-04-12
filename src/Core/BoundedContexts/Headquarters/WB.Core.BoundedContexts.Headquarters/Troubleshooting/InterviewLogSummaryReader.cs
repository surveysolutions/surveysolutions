using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Troubleshooting
{
    internal class InterviewLogSummaryReader : IInterviewLogSummaryReader
    {
        private readonly IPlainStorageAccessor<SynchronizationLogItem> plainStorageAccessor;

        public InterviewLogSummaryReader(IPlainStorageAccessor<SynchronizationLogItem> plainStorageAccessor)
        {
            this.plainStorageAccessor = plainStorageAccessor;
        }

        public InterviewSyncLogSummary GetInterviewLog(Guid interviewId, Guid responsibleId)
        {
            var interviewSynchronizationLog = this.plainStorageAccessor.Query(queryable => queryable
                .Where(x => (x.Type == SynchronizationLogType.PostInterview || x.Type == SynchronizationLogType.GetInterview) && x.InterviewId == interviewId)
                .OrderByDescending(x => x.LogDate)
                .ToList());

            DateTime? lastUploadInterviewDate = interviewSynchronizationLog.LastOrDefault(x => x.Type == SynchronizationLogType.PostInterview)?.LogDate;
            DateTime? lastDownloadInterviewDate = interviewSynchronizationLog.FirstOrDefault(x => x.Type == SynchronizationLogType.GetInterview)?.LogDate;
            DateTime? firstDownloadInterviewDate = interviewSynchronizationLog.LastOrDefault(x => x.Type == SynchronizationLogType.GetInterview)?.LogDate;

            DateTime? lastLinkDate = this.plainStorageAccessor.Query(queryable => queryable
              .Where(x => x.Type == SynchronizationLogType.LinkToDevice && x.InterviewerId == responsibleId)
              .OrderByDescending(x => x.LogDate)
              .Take(1)
              .ToList()).SingleOrDefault()?.LogDate;

            return new InterviewSyncLogSummary
            {
                FirstDownloadInterviewDate = firstDownloadInterviewDate,
                LastDownloadInterviewDate = lastDownloadInterviewDate,
                LastLinkDate = lastLinkDate,
                LastUploadInterviewDate = lastUploadInterviewDate
            };
        }
    }
}
