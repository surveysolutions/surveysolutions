using System;
using System.Threading;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class MapSyncProgressStatus
    {
        public MapSyncProgressStatus(Progress<MapSyncProgress> progress, CancellationTokenSource cancellationToken)
        {
            this.Progress = progress;
            this.CancellationTokenSource = cancellationToken;
        }

        public Progress<MapSyncProgress> Progress { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }
    }
}
