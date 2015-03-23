using System;
using System.IO;
using Android.App;
using Android.Content;

using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Syncronization.Update
{
    public class UpdateProcessor
    {
        private const string DownloadFolder = "download";
        private readonly string pathToFolder = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, DownloadFolder);
        private readonly ILogger logger;
        private readonly ISynchronizationService synchronizationService;

        public UpdateProcessor(ILogger logger, ISynchronizationService synchronizationService)
        {
            this.logger = logger;
            this.synchronizationService = synchronizationService;

            if (!Directory.Exists(this.pathToFolder))
            {
                Directory.CreateDirectory(this.pathToFolder);
            }
        }

        public void GetLatestVersion(Uri uri, string fileName)
        {
            try
            {
                string pathTofile = Path.Combine(this.pathToFolder, fileName);
                // generate unique name instead of delete
                if (File.Exists(pathTofile))
                    File.Delete(pathTofile);

                var client = new System.Net.WebClient();
                client.DownloadFile(uri, pathTofile);
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
            bool? newVersionAvailableOrNullIfThrow = null;
            try
            {
                newVersionAvailableOrNullIfThrow = this.synchronizationService.NewVersionAvailableAsync().Result;
            }
            catch
            {
            }
            return newVersionAvailableOrNullIfThrow;
        }
    }
}