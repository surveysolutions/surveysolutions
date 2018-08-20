using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public interface IInterviewsRemover
    {
        void RemoveInterviews(SynchronizationStatistics contextStatistics,
            IProgress<SyncProgressInfo> contextProgress, params Guid[] localInterviewIdsToRemove);
    }
}
