using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISynchronizationProcess
    {
        Task SynchronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken);
    }

    public static class SyncProgressHelper
    {
        public static IProgress<TransferProgress> AsTransferReport(this IProgress<SyncProgressInfo> syncProgress)
        {
            return new Progress<TransferProgress>(data
                => syncProgress?.Report(new SyncProgressInfo { TransferProgress = data }));
        }
    }
}
