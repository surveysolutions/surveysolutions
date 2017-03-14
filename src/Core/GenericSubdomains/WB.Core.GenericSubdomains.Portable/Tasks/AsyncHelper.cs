using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Tasks
{
    /// <summary>
    /// Copy/paste from AsyncHelper in Microsoft.AspNet.Identity.Core
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}