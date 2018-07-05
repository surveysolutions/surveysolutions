using System;
using System.Threading;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Services.MapSynchronization
{
    public class MapSyncProgressStatus
    {
        public MapSyncProgressStatus(Progress<SyncProgressInfo> progress, CancellationTokenSource cancellationToken)
        {
            this.Progress = progress;
            this.CancellationTokenSource = cancellationToken;
        }

        public Progress<SyncProgressInfo> Progress { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }
    }
}
