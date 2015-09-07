using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Interviewer.Syncronization.Update
{
    public class UpdateProcessor
    {
        private const string DownloadFolder = "download";
        private readonly string pathToFolder = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, DownloadFolder);
        private readonly ILogger logger;
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerSettings interviewerSettings;

        public UpdateProcessor(ILogger logger, ISynchronizationService synchronizationService, IInterviewerSettings interviewerSettings)
        {
            this.logger = logger;
            this.synchronizationService = synchronizationService;
            this.interviewerSettings = interviewerSettings;

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

                var client = new WebClient();
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

        public async Task<bool?> CheckNewVersion()
        {
            bool? newVersionAvailableOrNullIfThrow = null;
            try
            {
                newVersionAvailableOrNullIfThrow = (await this.synchronizationService.GetLatestApplicationVersionAsync()).Value > this.interviewerSettings.GetApplicationVersionCode();
            }
            catch
            {
            }
            return newVersionAvailableOrNullIfThrow;
        }
    }
}