using System;
using System.IO;
using Android.App;
using Android.Content;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Syncronization.RestUtils;

namespace WB.UI.Capi.Syncronization.Update
{
    public class UpdateProcessor
    {
        private const string downloadFolder = "download";
        private string pathToFolder = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, downloadFolder);
        
        private ILogger logger;

        public UpdateProcessor()
        {
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();

            if (!Directory.Exists(this.pathToFolder))
            {
                Directory.CreateDirectory(this.pathToFolder);
            }
        }

        public void GetLatestVersion(string url, string fileName)
        {
            try
            {
                string pathTofile = Path.Combine(this.pathToFolder, fileName);
                // generate unique name instead of delete
                if (File.Exists(pathTofile))
                    File.Delete(pathTofile);

                var client = new System.Net.WebClient();
                client.DownloadFile(url, pathTofile);
            }
            catch (Exception ex)
            {
                this.logger.Error("Error on file download", ex);
                throw;
            }
        }

        public void StartUpdate(string fileName)
        {
            string pathToFile = Path.Combine(this.pathToFolder, fileName);

            if (File.Exists(pathToFile))
            {
                Intent promptInstall = 
                    new Intent(Intent.ActionView).SetDataAndType(global::Android.Net.Uri.FromFile(new Java.IO.File(pathToFile)), 
                                                                 "application/vnd.android.package-archive");
                promptInstall.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(promptInstall);
            }
        }

        public bool? CheckNewVersion()
        {
            var executor = new AndroidRestUrils(SettingsManager.GetSyncAddressPoint());
            var checker = new RestVersionUpdate(executor);
            return checker.Execute(SettingsManager.AppVersionName(), SettingsManager.AppVersionCode(), SettingsManager.AndroidVersion());
        }
    }
}