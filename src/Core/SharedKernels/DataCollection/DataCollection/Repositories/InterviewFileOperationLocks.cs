using System;
using System.Collections.Concurrent;
using System.IO;
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
            // Keep the durable cross-process lock set bounded while preserving enough stripes to avoid frequent collisions.
            private const int CrossProcessLockStripeCount = 256;
            private const int InitialCrossProcessLockRetryDelayMs = 25;
            private const int MaxCrossProcessLockRetryDelayMs = 250;
            private static readonly string lockDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WB",
                "InterviewFileOperationLocks");
            private readonly Guid interviewId;
            private readonly LockEntry entry;
            private FileStream crossProcessLockStream;
            private bool released;

            internal InterviewFileOperationLock(Guid interviewId, LockEntry entry)
            {
                this.interviewId = interviewId;
                this.entry = entry;
            }

            public async Task WaitAsync()
            {
                await this.entry.Semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    this.crossProcessLockStream = await AcquireCrossProcessLockStream().ConfigureAwait(false);
                }
                catch
                {
                    this.entry.Semaphore.Release();
                    throw;
                }
            }

            public void Release()
            {
                if (released)
                    return;

                released = true;
                this.crossProcessLockStream?.Dispose();
                this.crossProcessLockStream = null;
                this.entry.Semaphore.Release();
            }

            public void Dispose()
            {
                try
                {
                    Release();
                }
                finally
                {
                    lock (sync)
                    {
                        entry.References--;
                        if (entry.References == 0 &&
                            locks.TryGetValue(interviewId, out var currentEntry) &&
                            ReferenceEquals(currentEntry, entry))
                        {
                            locks.TryRemove(interviewId, out _);
                        }
                    }
                }
            }

            private async Task<FileStream> AcquireCrossProcessLockStream()
            {
                Directory.CreateDirectory(lockDirectoryPath);

                var lockPath = Path.Combine(lockDirectoryPath, GetCrossProcessLockKey());
                var retryDelayMs = InitialCrossProcessLockRetryDelayMs;
                while (true)
                {
                    try
                    {
                        return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    }
                    catch (IOException)
                    {
                        await Task.Delay(retryDelayMs).ConfigureAwait(false);
                        retryDelayMs = Math.Min(retryDelayMs * 2, MaxCrossProcessLockRetryDelayMs);
                    }
                }
            }

            private string GetCrossProcessLockKey() =>
                $"{GetStableStripeIndex(this.interviewId):x2}.lck";

            private static int GetStableStripeIndex(Guid interviewId)
            {
                unchecked
                {
                    uint hash = 2166136261;
                    foreach (var value in interviewId.ToByteArray())
                        hash = (hash ^ value) * 16777619;

                    return (int)(hash % CrossProcessLockStripeCount);
                }
            }
        }
    }
}
