using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class UserInterfaceStateService : IUserInterfaceStateService
    {
        private static int count = 0;
        private static int throttledActionsCount = 0;

        public void NotifyRefreshStarted()
        {
            Interlocked.Increment(ref count);
        }

        public void NotifyRefreshFinished()
        {
            if (count > 0)
            {
                Interlocked.Decrement(ref count);
            }
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

        public void ThrottledActionStarted()
        {
            Interlocked.Increment(ref throttledActionsCount);
        }

        public void ThrottledActionFinished()
        {
            if (throttledActionsCount > 0)
            {
                Interlocked.Decrement(ref throttledActionsCount);
            }
        }

        public bool HasPendingThrottledActions => throttledActionsCount > 0;

        public bool IsUserInterfaceLocked => count > 0;
    }
}
