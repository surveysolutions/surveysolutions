using System.Globalization;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Java.IO;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class ExternalAppLauncher : IExternalAppLauncher
    {
        private Activity CurrentActivity => ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>().Activity;

        public void LaunchMapsWithTargetLocation(double latitude, double longitude)
        {
            var geoUri = Android.Net.Uri.Parse(
                string.Format("geo:{0},{1}?q={0},{1}(Target+Location)", latitude.ToString(CultureInfo.InvariantCulture), longitude.ToString(CultureInfo.InvariantCulture)));

            var mapIntent = new Intent(Intent.ActionView, geoUri);

            this.CurrentActivity.StartActivity(mapIntent);
        }

        public void OpenPdf(string pathToPdfFile)
        {
            var pdfIntent = new Intent(Intent.ActionView);

            var pdfFile = new File(pathToPdfFile);
            var uriForPdfFile = FileProvider.GetUriForFile(this.CurrentActivity,
                $"{this.CurrentActivity.ApplicationContext.PackageName}.fileprovider", pdfFile);

            pdfIntent.SetDataAndType(uriForPdfFile, "application/pdf")
                .SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask)
                .SetFlags(ActivityFlags.GrantReadUriPermission);

            this.CurrentActivity.StartActivity(pdfIntent);
        }
    }
}
