using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public interface IAssignmentsSynchronizer
    {
        Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress,
            SychronizationStatistics statistics, CancellationToken cancellationToken);
    }
}