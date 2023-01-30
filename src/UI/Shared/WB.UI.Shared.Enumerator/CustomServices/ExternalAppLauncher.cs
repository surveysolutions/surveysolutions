using System.Globalization;
using Android.Content;
using AndroidX.Core.Content;
using Java.IO;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    internal class ExternalAppLauncher : IExternalAppLauncher
    {
        private readonly IMvxAndroidCurrentTopActivity currentTopActivity;

        public ExternalAppLauncher()
        {
            this.currentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
        }

        public void LaunchMapsWithTargetLocation(double latitude, double longitude)
        {
            var geoUri = Android.Net.Uri.Parse(
                string.Format("geo:{0},{1}?q={0},{1}(Target+Location)", latitude.ToString(CultureInfo.InvariantCulture), longitude.ToString(CultureInfo.InvariantCulture)));

            var mapIntent = new Intent(Intent.ActionView, geoUri);

            if(this.currentTopActivity.Activity.PackageManager != null && mapIntent.ResolveActivity(this.currentTopActivity.Activity.PackageManager) != null)
                this.currentTopActivity.Activity.StartActivity(mapIntent);
        }

        public void OpenPdf(string pathToPdfFile)
        {
            var pdfIntent = new Intent(Intent.ActionView);

            var pdfFile = new Java.IO.File(pathToPdfFile);
            var uriForPdfFile = FileProvider.GetUriForFile(this.currentTopActivity.Activity,
                $"{this.currentTopActivity.Activity.ApplicationContext.PackageName}.fileprovider", pdfFile);

            pdfIntent.SetDataAndType(uriForPdfFile, "application/pdf")
                .SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask)
                .SetFlags(ActivityFlags.GrantReadUriPermission);

            if(this.currentTopActivity.Activity.PackageManager != null && pdfIntent.ResolveActivity(this.currentTopActivity.Activity.PackageManager) != null)
                this.currentTopActivity.Activity.StartActivity(pdfIntent);
        }
    }
}
