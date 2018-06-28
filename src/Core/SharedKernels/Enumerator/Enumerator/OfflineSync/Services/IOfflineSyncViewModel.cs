using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        Task OnGoogleApiReady();
    }
}
