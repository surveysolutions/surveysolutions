using System.Collections.Generic;
using System.Threading;

namespace WB.Core.Infrastructure.Storage
{
    public static class IsolatedThreadManager
    {
        private static readonly HashSet<int> IsolatedThreads = new HashSet<int>();

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