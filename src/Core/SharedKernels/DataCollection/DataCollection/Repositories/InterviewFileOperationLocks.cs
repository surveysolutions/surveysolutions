using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public static class InterviewFileOperationLocks
    {
        private static readonly ConcurrentDictionary<Guid, LockEntry> locks = new();
        private static readonly object sync = new();

        public static InterviewFileOperationLock Get(Guid interviewId)
        {
            lock (sync)
            {
                var entry = locks.GetOrAdd(interviewId, _ => new LockEntry());
                entry.References++;
                return new InterviewFileOperationLock(interviewId, entry);
            }
        }

        internal sealed class LockEntry
        {
            public readonly SemaphoreSlim Semaphore = new(1, 1);
            public int References;
        }

        public sealed class InterviewFileOperationLock : IDisposable
        {
            private readonly Guid interviewId;
            private readonly LockEntry entry;
            private bool released;

            internal InterviewFileOperationLock(Guid interviewId, LockEntry entry)
            {
                this.interviewId = interviewId;
                this.entry = entry;
            }

            public Task WaitAsync() => entry.Semaphore.WaitAsync();

            public void Release()
            {
                if (released)
                    return;

                released = true;
                entry.Semaphore.Release();
            }

            public void Dispose()
            {
                Release();
                lock (sync)
                {
                    entry.References--;
                    if (entry.References == 0 &&
                        locks.TryGetValue(interviewId, out var currentEntry) &&
                        ReferenceEquals(currentEntry, entry))
                    {
                        locks.TryRemove(interviewId, out _);
                        entry.Semaphore.Dispose();
                    }
                }
            }
        }
    }
}
