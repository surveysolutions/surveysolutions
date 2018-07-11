using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncViewModel
    {
        void SetGoogleAwaiter(Task<bool> apiConnected);
    }
}
