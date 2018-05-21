using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public interface IAuditLogSynchronizer
    {
        Task SynchronizeAuditLogAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics, CancellationToken cancellationToken);
    }
}
