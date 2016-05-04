using System.Threading;

namespace WB.Infrastructure.Native.Threading
{
    public static class IsolatedThreadExtensions
    {
        public static Thread AsIsolatedThread(this Thread thread)
        {
            return ThreadMarkerManager.IsIsolated(thread) ? thread : null;
        }
    }
}