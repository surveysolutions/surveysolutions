using System;
using Android.Gms.Common;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class GoogleApiService : IGoogleApiService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;

        public GoogleApiService()
        {
            this.androidCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();;
        }

        public GoogleApiConnectionStatus GetPlayServicesConnectionStatus() =>
            (GoogleApiConnectionStatus)GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.androidCurrentTopActivity.Activity);

        public bool CanResolvePlayServicesErrorByUser(GoogleApiConnectionStatus errorCode) =>
            GoogleApiAvailability.Instance.IsUserResolvableError((int)errorCode);

        public void ShowGoogleApiErrorDialog(GoogleApiConnectionStatus errorCode, int requestCode, Action onCancel = null) =>
            GoogleApiAvailability.Instance.GetErrorDialog(this.androidCurrentTopActivity.Activity, (int)errorCode, requestCode).Show();
    }
}
