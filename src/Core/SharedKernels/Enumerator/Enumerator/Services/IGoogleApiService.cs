using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IGoogleApiService
    {
        GoogleApiConnectionStatus GetPlayServicesConnectionStatus();
        bool CanResolvePlayServicesErrorByUser(GoogleApiConnectionStatus errorCode);
        void ShowGoogleApiErrorDialog(GoogleApiConnectionStatus errorCode, int requestCode, Action onCancel = null);
    }
}
