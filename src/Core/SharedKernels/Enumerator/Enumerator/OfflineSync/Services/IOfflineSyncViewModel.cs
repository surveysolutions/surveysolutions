using System.Threading.Tasks;
using MvvmCross.Commands;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        void SetGoogleAwaiter(Task<bool> apiConnected);

        IMvxCommand CancelCommand { get; }
    }
}
