using System;
using System.Collections.Concurrent;
using System.Threading;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public static class InterviewFileOperationLocks
    {
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> locks = new();

        public static SemaphoreSlim Get(Guid interviewId) =>
            locks.GetOrAdd(interviewId, _ => new SemaphoreSlim(1, 1));
    }
}
