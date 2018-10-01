using System;
using System.Threading;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class EnumeratorSynchonizationContext
    {
        public SynchronizationStatistics Statistics { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public IProgress<SyncProgressInfo> Progress { get; set; }
    }
}