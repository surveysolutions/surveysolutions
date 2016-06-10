using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IUserInterfaceStateService
    {
        void NotifyRefreshStarted();

        void NotifyRefreshFinished();

        Task WaitWhileUserInterfaceIsRefreshingAsync();

        bool IsUserInferfaceLocked { get; }
    }
}