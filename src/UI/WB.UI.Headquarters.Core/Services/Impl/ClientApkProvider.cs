using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Services.Impl
{
    public class ClientApkProvider : IClientApkProvider
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly string ClientAppsFolder;

        public ClientApkProvider(
            IAndroidPackageReader androidPackageReader,
            IFileSystemAccessor fileSystemAccessor,
            IOptions<ApkConfig> settingsProvider)
        {
            this.androidPackageReader = androidPackageReader;
            this.fileSystemAccessor = fileSystemAccessor;
            this.ClientAppsFolder = settingsProvider.Value.ClientApkPath;
        }

        public IActionResult GetApkAsHttpResponse(HttpRequest request, string appName, string responseFileName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(ApkClientsFolder(), appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return new NotFoundObjectResult(TabletSyncMessages.FileWasNotFound);

            var fileHash = this.fileSystemAccessor.ReadHash(pathToInterviewerApp);

            if (request.RequestHasMatchingFileHash(fileHash))
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            Stream fileStream = new FileStream(pathToInterviewerApp, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, "application/vnd.android.package-archive")
            {
                FileDownloadName = responseFileName,
                EnableRangeProcessing = true,
                EntityTag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(fileHash)}\"")
            };
        }

        public IActionResult GetPatchFileAsHttpResponse(HttpRequest request, string fileName)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                ApkClientsFolder(), fileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return new NotFoundObjectResult(TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(fileStream, "application/vnd.android.package-archive")
            {
                EnableRangeProcessing = true,
                EntityTag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(this.fileSystemAccessor.ReadHash(pathToInterviewerPatch))}\"")
            };
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
            return this.ClientAppsFolder;
        }
    }
}
