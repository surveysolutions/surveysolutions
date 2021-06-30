using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptionsSnapshot<ApkConfig> options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<ClientApkProvider> logger;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly IMemoryCache appVersionCache;
        
        
        public ClientApkProvider(
            IAndroidPackageReader androidPackageReader,
            IFileSystemAccessor fileSystemAccessor,
            IOptionsSnapshot<ApkConfig> options,
            IHttpClientFactory httpClientFactory,
            ILogger<ClientApkProvider> logger,
            IMemoryCache appVersionCache)
        {
            this.androidPackageReader = androidPackageReader;
            this.fileSystemAccessor = fileSystemAccessor;
            this.options = options;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.appVersionCache = appVersionCache;
        }

        public async Task<IActionResult> GetApkAsHttpResponse(HttpRequest request, string appName,
            string responseFileName)
        {
            var directory = ApkClientsFolder();
            
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(directory, appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
            {
                if (Uri.TryCreate(directory, UriKind.Absolute, out Uri uri) && !uri.IsFile)
                {
                    return await GetApkFromRemoteServer(request, appName, responseFileName, uri);
                }

                return new NotFoundObjectResult(TabletSyncMessages.FileWasNotFound);
            }

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

        private async Task<IActionResult> GetApkFromRemoteServer(HttpRequest request, string fileName, string responseFileName, Uri uri)
        {
            HttpClient apkClient = httpClientFactory.CreateClient("apks");
            apkClient.BaseAddress = uri;

            var requestUri = $"/{fileName}";
            
            HttpResponseMessage body = await apkClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
            if (body.StatusCode != HttpStatusCode.OK)
            {
                return new StatusCodeResult((int) body.StatusCode);
            }
            
            var responseStream = await body.Content.ReadAsStreamAsync();
            var hash = this.fileSystemAccessor.ReadHash(responseStream);
            
            if (request.RequestHasMatchingFileHash(hash))
            {
                return new StatusCodeResult(StatusCodes.Status304NotModified);
            }

            responseStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(responseStream, "application/vnd.android.package-archive")
            {
                FileDownloadName = responseFileName,
                EnableRangeProcessing = true,
                EntityTag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(hash)}\"")
            };
        }

        public async Task<IActionResult> GetPatchFileAsHttpResponse(HttpRequest request, string fileName)
        {
            var apkClientsFolder = ApkClientsFolder();
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                apkClientsFolder, fileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
            {
                if (Uri.TryCreate(apkClientsFolder, UriKind.Absolute, out Uri uri) && !uri.IsFile)
                {
                    return await GetApkFromRemoteServer(request, fileName, fileName, uri);
                }
                return new NotFoundObjectResult(TabletSyncMessages.FileWasNotFound);
            }

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(fileStream, "application/vnd.android.package-archive")
            {
                EnableRangeProcessing = true,
                EntityTag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(this.fileSystemAccessor.ReadHash(pathToInterviewerPatch))}\"")
            };
        }

        public async Task<int?> GetApplicationBuildNumber(string appName)
        {
            var apkClientsFolder = ApkClientsFolder();
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(
                apkClientsFolder, appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
            {
                if (Uri.TryCreate(apkClientsFolder, UriKind.Absolute, out Uri uri) && !uri.IsFile)
                {
                    var version = await GetApplicationVersionFromRemoteServer(uri, appName);
                    return version?.BuildNumber;
                }

                return null;
            }

            return this.androidPackageReader.Read(pathToInterviewerApp).BuildNumber;
        }

        private async Task<AndroidPackageInfo> GetApplicationVersionFromRemoteServer(Uri remoteUri, string appName)
        {
            HttpClient apkClient = httpClientFactory.CreateClient("apks");
            try
            {
                apkClient.BaseAddress = remoteUri;
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, "Failed to receive app version from {path}", remoteUri);
                return null;
            }

            var requestUri = appName;
            var headResponse = await apkClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestUri));
            if (headResponse.StatusCode != HttpStatusCode.OK)
            {
                this.logger.LogError("Failed to receive app version from {path}. Server responded with {responseCode}", remoteUri, headResponse.StatusCode);
                return null;
            }

            var remoteEtag = headResponse.Headers.ETag.Tag;

            var cacheKey = remoteEtag + "build";
            if(this.appVersionCache.TryGetValue(cacheKey, out AndroidPackageInfo cachedVersion))
            {
                return cachedVersion;
            }

            HttpResponseMessage body = await apkClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
            var responseStream = await body.Content.ReadAsStreamAsync();
            AndroidPackageInfo packageInfo = this.androidPackageReader.Read(responseStream);

            
            this.appVersionCache.Set(cacheKey, packageInfo, TimeSpan.FromMinutes(5));
            return packageInfo;
        }
        
        public async Task<string> GetApplicationVersionString(string appName)
        {
            var apkClientsFolder = ApkClientsFolder();
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(
                apkClientsFolder, appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
            {
                if (Uri.TryCreate(apkClientsFolder, UriKind.Absolute, out Uri uri) && !uri.IsFile)
                {
                    var version = await GetApplicationVersionFromRemoteServer(uri, appName);
                    return version?.VersionString;
                }
                
                return null;
            }

            return this.androidPackageReader.Read(pathToInterviewerApp).VersionString;
        }

        public string ApkClientsFolder()
        {
            return this.options.Value.ClientApkPath;
        }
    }
}
