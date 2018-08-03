using System.Globalization;
using Android.App;
using Android.Content;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class ExternalAppLauncher : IExternalAppLauncher
    {
        private Activity CurrentActivity
        {
            get { return Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity; }
        }

        public void LaunchMapsWithTargetLocation(double latitude, double longitude)
        {
            var geoUri = Android.Net.Uri.Parse(
                string.Format("geo:{0},{1}?q={0},{1}(Target+Location)", latitude.ToString(CultureInfo.InvariantCulture), longitude.ToString(CultureInfo.InvariantCulture)));

            var mapIntent = new Intent(Intent.ActionView, geoUri);

            this.CurrentActivity.StartActivity(mapIntent);
        }
    }
}
