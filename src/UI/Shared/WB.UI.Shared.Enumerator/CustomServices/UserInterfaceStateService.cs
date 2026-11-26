using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class UserInterfaceStateService : IUserInterfaceStateService
    {
        private static int count = 0;
        private static int throttledActionsCount = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyRefreshStarted()
        {
            if (Interlocked.Increment(ref count) == 1)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsUserInterfaceLocked)));
        }

        public void NotifyRefreshFinished()
        {
            int currentCount;
            do
            {
                currentCount = Volatile.Read(ref count);
                if (currentCount == 0)
                    return;
            } while (Interlocked.CompareExchange(ref count, currentCount - 1, currentCount) != currentCount);

            if (currentCount == 1)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsUserInterfaceLocked)));
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
