using System.Collections.Generic;
using System.Threading;

namespace WB.Core.Infrastructure.Storage
{
    public static class NoTransactionalThreadMarkerManager
    {
        private static readonly HashSet<int> NoTransactionMarkedThreads = new HashSet<int>();

        public static void MarkCurrentThreadAsNoTransactional()
        {
            NoTransactionMarkedThreads.Add(Thread.CurrentThread.ManagedThreadId);
        }

        public static void ReleaseCurrentThreadAsNoTransactional()
        {
            NoTransactionMarkedThreads.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public static bool IsMarkedAsNoTransaction()
        {
            return NoTransactionMarkedThreads.Contains(Thread.CurrentThread.ManagedThreadId);
        }
    }
}