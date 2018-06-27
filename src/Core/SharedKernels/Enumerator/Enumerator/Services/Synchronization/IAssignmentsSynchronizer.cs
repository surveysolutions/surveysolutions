using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface IAssignmentsSynchronizer
    {
        Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken);
    }
}
