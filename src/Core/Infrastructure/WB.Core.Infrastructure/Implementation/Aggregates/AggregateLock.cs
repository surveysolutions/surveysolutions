using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateLock : IAggregateLock
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new();
        private readonly ConditionalWeakTable<IUnitOfWorkCompletion, TransactionLocks> transactionLocks = new();
        private readonly ThreadLocal<TransactionLocks> currentTransactionLocks = new();
        private readonly ThreadLocal<HashSet<string>> currentlyHeldLocks =
            new(() => new HashSet<string>(StringComparer.Ordinal));

        public T RunWithLock<T>(string aggregateGuid, Func<T> run)
        {
            if (currentlyHeldLocks.Value.Contains(aggregateGuid))
                return run();

            var transaction = currentTransactionLocks.Value;
            if (transaction != null)
            {
                transaction.Acquire(aggregateGuid, GetSemaphore(aggregateGuid));
                return RunWithCurrentLock(aggregateGuid, run);
            }

            var semaphore = GetSemaphore(aggregateGuid);
            semaphore.Wait();
            try
            {
                return RunWithCurrentLock(aggregateGuid, run);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void RunWithLock(string aggregateGuid, Action run)
        {
            RunWithLock(aggregateGuid, () =>
            {
                run();
                return true;
            });
        }

        public void RunWithLock(string aggregateGuid, IUnitOfWorkCompletion unitOfWork, Action run)
        {
            var transaction = transactionLocks.GetValue(unitOfWork, key =>
            {
                var locks = new TransactionLocks();
                key.OnCompleted(_ => locks.ReleaseAll());
                return locks;
            });

            var previousTransaction = currentTransactionLocks.Value;
            currentTransactionLocks.Value = transaction;
            try
            {
                transaction.Acquire(aggregateGuid, GetSemaphore(aggregateGuid));
                RunWithCurrentLock(aggregateGuid, run);
            }
            finally
            {
                currentTransactionLocks.Value = previousTransaction;
            }
        }

        private SemaphoreSlim GetSemaphore(string aggregateGuid) =>
            semaphores.GetOrAdd(aggregateGuid, _ => new SemaphoreSlim(1, 1));

        private T RunWithCurrentLock<T>(string aggregateGuid, Func<T> run)
        {
            var heldLocks = currentlyHeldLocks.Value;
            heldLocks.Add(aggregateGuid);
            try
            {
                return run();
            }
            finally
            {
                heldLocks.Remove(aggregateGuid);
            }
        }

        private void RunWithCurrentLock(string aggregateGuid, Action run)
        {
            RunWithCurrentLock(aggregateGuid, () =>
            {
                run();
                return true;
            });
        }

        private sealed class TransactionLocks
        {
            private readonly object sync = new();
            private readonly Dictionary<string, SemaphoreSlim> heldLocks = new();

            public bool Acquire(string key, SemaphoreSlim semaphore)
            {
                lock (sync)
                {
                    if (heldLocks.ContainsKey(key))
                        return false;

                    if (lastAcquiredKey != null && string.CompareOrdinal(key, lastAcquiredKey) < 0)
                        throw new InvalidOperationException("Aggregate locks must be acquired in ascending order.");

                    semaphore.Wait();
                    heldLocks.Add(key, semaphore);
                    lastAcquiredKey = key;
                    return true;
                }
            }

            public void ReleaseAll()
            {
                lock (sync)
                {
                    foreach (var semaphore in heldLocks.Values)
                        semaphore.Release();

                    heldLocks.Clear();
                    lastAcquiredKey = null;
                }
            }

            private string lastAcquiredKey;
        }
    }
}
