using System.Collections.Generic;
using System.Threading;

namespace WB.Core.Infrastructure.Storage
{
    public static class ThreadMarkerManager
    {
        private static readonly HashSet<int> NoTransactionMarkedThreads = new HashSet<int>();
        private static readonly HashSet<int> IsolatedThreads = new HashSet<int>();

        public static void MarkCurrentThreadAsNoTransactional()
        {
            NoTransactionMarkedThreads.Add(Thread.CurrentThread.ManagedThreadId);
        }

        public static void RemoveCurrentThreadFromNoTransactional()
        {
            NoTransactionMarkedThreads.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public static bool IsCurrentThreadNoTransactional()
        {
            return NoTransactionMarkedThreads.Contains(Thread.CurrentThread.ManagedThreadId);
        }
        
        public static void MarkCurrentThreadAsIsolated()
        {
            IsolatedThreads.Add(Thread.CurrentThread.ManagedThreadId);
        }

        public static void ReleaseCurrentThreadFromIsolation()
        {
            IsolatedThreads.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public static bool IsIsolated(Thread thread)
        {
            return IsolatedThreads.Contains(thread.ManagedThreadId);
        }
    }
}