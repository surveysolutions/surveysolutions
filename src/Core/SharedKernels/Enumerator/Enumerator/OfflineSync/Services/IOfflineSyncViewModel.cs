using System.Threading.Tasks;
using MvvmCross.Commands;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        Task OnGoogleApiReady();

        IMvxCommand CancelCommand { get; }
    }
}
