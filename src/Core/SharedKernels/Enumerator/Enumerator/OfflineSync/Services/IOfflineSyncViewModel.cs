using System.Threading.Tasks;
using MvvmCross.Commands;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        IMvxAsyncCommand StartSynchronization { get; }
        IMvxCommand CancelCommand { get; }
    }
}
