using System.Threading;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Infrastructure.Native
{
    public static class ThreadMarkerManager
    {
        private static readonly ConcurrentHashSet<int> NoTransactionMarkedThreads = new ConcurrentHashSet<int>();
        private static readonly ConcurrentHashSet<int> IsolatedThreads = new ConcurrentHashSet<int>();

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