using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class UserInterfaceStateService : IUserInterfaceStateService
    {
        private static int count = 0;

        public void NotifyRefreshStarted()
        {
            Interlocked.Increment(ref count);
        }

        public void NotifyRefreshFinished()
        {
            Interlocked.Decrement(ref count);
        }

        public Task WaitWhileUserInterfaceIsRefreshingAsync()
        {
            return Task.Run((async () =>
            {
                while (count > 0)
                {
                    await Task.Delay(100);
                }
            }));
        }
    }
}