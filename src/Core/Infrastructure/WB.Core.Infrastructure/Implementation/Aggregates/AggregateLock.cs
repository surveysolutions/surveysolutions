using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    public class AggregateLock : IAggregateLock
    {
        private readonly NamedLocker locker = new NamedLocker();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new();
        private readonly ConditionalWeakTable<IUnitOfWorkCompletion, TransactionLocks> transactionLocks = new();
        
        public T RunWithLock<T>(string aggregateGuid, Func<T> run)
        {
            return this.locker.RunWithLock(aggregateGuid, run);
        }

        public void RunWithLock(string aggregateGuid, Action run)
        {
            this.locker.RunWithLock(aggregateGuid, run);
        }

        public void RunWithLock(string aggregateGuid, IUnitOfWorkCompletion unitOfWork, Action run)
        {
            var transaction = transactionLocks.GetValue(unitOfWork, key =>
            {
                var locks = new TransactionLocks();
                key.OnCompleted(_ => locks.ReleaseAll());
                return locks;
            });

            if (!transaction.Acquire(aggregateGuid, semaphores.GetOrAdd(aggregateGuid, _ => new SemaphoreSlim(1, 1))))
            {
                run();
                return;
            }

            run();
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

                    semaphore.Wait();
                    heldLocks.Add(key, semaphore);
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
                }
            }
        }
    }
}
