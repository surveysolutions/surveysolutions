using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        IMvxAsyncCommand StartDiscoveryAsyncCommand { get; }
        IMvxCommand CancelCommand { get; }
        IUserInteractionService UserInteractionService { get; }

        IGoogleApiService GoogleApiService { get; }
    }
}
