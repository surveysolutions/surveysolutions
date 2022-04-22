using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using File = Google.Apis.Drive.v3.Data.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class GoogleDriveDataClient : IExternalDataClient
    {
        private readonly ILogger<GoogleDriveDataClient> logger;
        private readonly ITenantContext tenantContext;
        private readonly AsyncRetryPolicy retry;

        public GoogleDriveDataClient(ILogger<GoogleDriveDataClient> logger, 
            ITenantContext tenantContext)
        {
            this.logger = logger;
            this.tenantContext = tenantContext;
            
            this.retry = Policy.Handle<GoogleApiException>(e => e.HttpStatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(2, async (exception, span) =>
                {
                    this.logger.LogError(exception, $"Unauthorized exception during request to Google Drive");

                    var newAccessToken = await this.tenantContext.Api
                        .GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType.GoogleDrive,
                            this.refreshToken).ConfigureAwait(false);

                    this.CreateClient(newAccessToken);

                });
        }
        
        private DriveService? driveService;


        private DriveService DriveService
        {
            set => driveService = value;
            get
            {
                if (driveService == null)
                    throw new InvalidOperationException("GoogleDriveDataClient was not initialized.");
                return driveService;
            }
        }


        private string refreshToken = String.Empty;
        private const string GoogleDriveFolderMimeType = "application/vnd.google-apps.folder";

        public void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant)
        {
            this.refreshToken = refreshToken;
            
            this.CreateClient(accessToken);
        }

        private void CreateClient(string accessToken)
        {
            this.driveService?.Dispose();
            
            var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
            {
                AccessToken = accessToken,
                ExpiresInSeconds = 3600,
                IssuedUtc = DateTime.UtcNow
            };
            
            var fakeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "fakeClientId",
                    ClientSecret = "fakeClientSecret"
                }
            });

            UserCredential credential = new UserCredential(fakeFlow, "fakeUserId", token);
            var serviceInitializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            };

            this.DriveService = new DriveService(serviceInitializer);
        }

        public async Task<string> CreateApplicationFolderAsync(string subFolder)
        {
            const string applicationFolderName = "Survey Solutions";
            var result = await GetOrCreateFolderAsync(applicationFolderName);
            result = await GetOrCreateFolderAsync(this.tenantContext.Tenant.Name, result.Id);
            if (subFolder != null)
            {
                result = await GetOrCreateFolderAsync(subFolder, result.Id);
            }
            return result.Id;
        }

        public async Task<string> CreateFolderAsync(string directory, string parentDirectory)
        {
            string[] folders = directory.Split('/');

            string parentFolder = parentDirectory;

            foreach (var folder in folders)
            {
                var parentFolderFile = await GetOrCreateFolderAsync(folder, parentFolder);
                parentFolder = parentFolderFile.Id;
            }

            return parentFolder;
        }

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            var fileMetadata = new File
            {
                Name = fileName,
                Parents = new List<string> { folder }
            };

            var file = await GetFileIdAsync(fileName, folder);
            if (file != null)
            {
                await this.retry.ExecuteAsync(() => DriveService.Files.Delete(file.Id).ExecuteAsync(cancellationToken));
            }

            await DriveService.Files.Create(fileMetadata, fileStream, "application/octet-stream").UploadAsync(cancellationToken);
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            var storageInfo = await this.retry.ExecuteAsync(() =>
            {
                var storageInfoRequest = DriveService.About.Get();
                storageInfoRequest.Fields = "storageQuota";

                return storageInfoRequest.ExecuteAsync();
            });
            if (storageInfo?.StorageQuota?.Limit == null) return null;

            return storageInfo.StorageQuota.Limit - storageInfo.StorageQuota.Usage ?? 0;
        }

        private Task<File?> GetFileIdAsync(string filename, string parentFolderId = "")
        {
            var query = SearchQuery(filename, parentFolderId);
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<File> GetOrCreateFolderAsync(string folder, string parentId = "")
        {
            return await this.GetFolderIdAsync(folder, parentId)
                   ?? await this.ExecuteCreateFolderAsync(folder, parentId);
        }

        private async Task<File> ExecuteCreateFolderAsync(string folder, string parentId = "")
        {
            var file = new File
            {
                Name = folder,
                MimeType = GoogleDriveFolderMimeType
            };

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                file.Parents = new List<string> { parentId };
            }

            return await retry.ExecuteAsync(() =>
            {
                var createRequest = DriveService.Files.Create(file);
                createRequest.Fields = "id";

                return createRequest.ExecuteAsync();
            });
        }

        private Task<File?> GetFolderIdAsync(string folderName, string parentFolderId = "")
        {
            var query = SearchQuery(folderName, parentFolderId);
            query += $" and mimeType = \'{GoogleDriveFolderMimeType}'";
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<File?> SearchForFirstOccurenceAsync(string query) =>
            (await retry.ExecuteAsync(() =>
            {
                var listRequest = this.DriveService.Files.List();

                listRequest.Q = query;
                listRequest.Fields = "files(id, name)";
                
                return listRequest.ExecuteAsync();
            })).Files.FirstOrDefault();

        private string SearchQuery(string name, string parentFolder)
        {
            var query = $"name = '{name}' and trashed = false";
            if (!string.IsNullOrEmpty(parentFolder)) query += $" and '{parentFolder}' in parents";
            return query;
        }
        
        public void Dispose()
        {
            this.driveService?.Dispose();
            this.driveService = null;
        }
    }
}
