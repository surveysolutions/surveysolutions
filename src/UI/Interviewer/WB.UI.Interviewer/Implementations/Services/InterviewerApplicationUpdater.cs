using System;
using System.IO;
using System.Net;
using Android.App;
using Android.Content;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class InterviewerApplicationUpdater : IInterviewerApplicationUpdater
    {
        const string ApplicationFileName = "interviewer.apk";
        const string SyncGetlatestVersion = "/api/InterviewerSync/GetLatestVersion";

        private const string DownloadFolder = "download";
        private readonly string pathToFolder = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, DownloadFolder);
        private readonly ILogger logger;

        private readonly IInterviewerSettings interviewerSettings;

        public InterviewerApplicationUpdater(ILogger logger, IInterviewerSettings interviewerSettings)
        {
            this.logger = logger;
            this.interviewerSettings = interviewerSettings;

            if (!Directory.Exists(this.pathToFolder))
            {
                Directory.CreateDirectory(this.pathToFolder);
            }
        }

        public void GetLatestVersion()
        {
            var uri = new Uri(new Uri(this.interviewerSettings.Endpoint), SyncGetlatestVersion);
            this.GetLatestVersion(uri, SyncGetlatestVersion);
            this.StartUpdate(ApplicationFileName);
        }

        private void GetLatestVersion(Uri uri, string fileName)
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

        private void StartUpdate(string fileName)
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
    }
}