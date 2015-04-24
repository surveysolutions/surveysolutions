using System.Threading;

namespace WB.Core.Infrastructure.Storage
{
    public static class IsolatedThreadExtensions
    {
        public static Thread AsIsolatedThread(this Thread thread)
        {
            return IsolatedThreadManager.IsIsolated(thread) ? thread : null;
        }
    }
}