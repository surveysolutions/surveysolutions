using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IUserInterfaceStateService
    {
        void NotifyRefreshStarted();

        void NotifyRefreshFinished();

        Task WaitWhileUserInterfaceIsRefreshingAsync();

        bool IsUserInterfaceLocked { get; }

        void ThrottledActionStarted();

        void ThrottledActionFinished();

        bool HasPendingThrottledActions { get; }
    }
}
