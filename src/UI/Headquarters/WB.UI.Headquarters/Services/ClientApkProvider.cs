using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Utils;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Services
{
    public class ClientApkProvider : IClientApkProvider
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly string ClientAppsFolder;

        public ClientApkProvider(
            IAndroidPackageReader androidPackageReader,
            IFileSystemAccessor fileSystemAccessor,
            ISettingsProvider settingsProvider)
        {
            this.androidPackageReader = androidPackageReader;
            this.fileSystemAccessor = fileSystemAccessor;
            var clientApkFolder = settingsProvider.AppSettings[@"ClientApkPath"];

            this.ClientAppsFolder = string.IsNullOrWhiteSpace(clientApkFolder)
                ? @"~/Client"
                : clientApkFolder;
        }

        public HttpResponseMessage GetApkAsHttpResponse(HttpRequestMessage request, string appName, string responseFileName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(ApkClientsFolder(), appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            var fileHash = this.fileSystemAccessor.ReadHash(pathToInterviewerApp);

            if (request.RequestHasMatchingFileHash(fileHash))
            {
                return request.CreateResponse(HttpStatusCode.NotModified);
            }

            Stream fileStream = new FileStream(pathToInterviewerApp, FileMode.Open, FileAccess.Read);

            return request.AsProgressiveDownload(fileStream,
                @"application/vnd.android.package-archive",
                responseFileName, fileHash);
        }

        public HttpResponseMessage GetPatchFileAsHttpResponse(HttpRequestMessage request, string fileName)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                ApkClientsFolder(), fileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return request.AsProgressiveDownload(fileStream, @"application/octet-stream",
                hash: this.fileSystemAccessor.ReadHash(pathToInterviewerPatch));
        }

        public int? GetApplicationBuildNumber(string appName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(
                ApkClientsFolder(), appName);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).BuildNumber;
        }

        public string GetApplicationVersionString(string appName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(
                ApkClientsFolder(), appName);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).VersionString;
        }

        public string ApkClientsFolder()
        {
            if (this.ClientAppsFolder.StartsWith("~")) return HostingEnvironment.MapPath(this.ClientAppsFolder);

            return this.ClientAppsFolder;
        }
    }
}
