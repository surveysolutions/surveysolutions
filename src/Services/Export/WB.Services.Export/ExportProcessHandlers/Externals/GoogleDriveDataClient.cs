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
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Polly;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using File = Google.Apis.Drive.v3.Data.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class GoogleDriveDataClient : IExternalDataClient
    {
        private readonly ILogger<GoogleDriveDataClient> logger;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        public GoogleDriveDataClient(ILogger<GoogleDriveDataClient> logger, 
            ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.logger = logger;
            this.tenantApi = tenantApi;
        }
        
        private DriveService driveService;
        private TenantInfo tenant;
        private string refreshToken;
        private const string GoogleDriveFolderMimeType = "application/vnd.google-apps.folder";

        public void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant)
        {
            this.tenant = tenant;
            this.refreshToken = refreshToken;
            
            this.CreateClient(accessToken);
        }

        private void CreateClient(string accessToken)
        {
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

            this.driveService = new DriveService(serviceInitializer);
        }

        public async Task<string> CreateApplicationFolderAsync(string subFolder)
        {
            const string applicationFolderName = "Survey Solutions";
            var result = await GetOrCreateFolderAsync(applicationFolderName);
            result = await GetOrCreateFolderAsync(this.tenant.Name, result.Id);
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
                Func<FilesResource.DeleteRequest> request = () => driveService.Files.Delete(file.Id);
                await this.ExecuteRequestAsync(request, cancellationToken);
            }

            await driveService.Files.Create(fileMetadata, fileStream, "application/octet-stream").UploadAsync(cancellationToken);
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            Func<ClientServiceRequest<About>> request = ()=>
            {
                var storageInfoRequest = driveService.About.Get();
                storageInfoRequest.Fields = "storageQuota";
                
                return storageInfoRequest;
            };

            var storageInfo = await this.ExecuteRequestAsync(request);
            if (storageInfo?.StorageQuota?.Limit == null) return null;

            return storageInfo.StorageQuota.Limit - storageInfo.StorageQuota.Usage ?? 0;
        }

        private Task<File> GetFileIdAsync(string filename, string parentFolderId = null)
        {
            var query = SearchQuery(filename, parentFolderId);
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<File> GetOrCreateFolderAsync(string folder, string parentId = null)
        {
            return await this.GetFolderIdAsync(folder, parentId)
                   ?? await this.ExecuteCreateFolderAsync(folder, parentId);
        }

        private async Task<File> ExecuteCreateFolderAsync(string folder, string parentId = null)
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

            return await this.ExecuteRequestAsync(() =>
            {
                var createRequest = driveService.Files.Create(file);
                createRequest.Fields = "id";

                return createRequest;
            });
        }

        private Task<File> GetFolderIdAsync(string folderName, string parentFolderId = null)
        {
            var query = SearchQuery(folderName, parentFolderId);
            query += $" and mimeType = \'{GoogleDriveFolderMimeType}'";
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<File> SearchForFirstOccurenceAsync(string query) =>
            (await this.ExecuteRequestAsync(() =>
            {
                var listRequest = this.driveService.Files.List();

                listRequest.Q = query;
                listRequest.Fields = "files(id, name)";
                return listRequest;
            })).Files.FirstOrDefault();

        private string SearchQuery(string name, string parentFolder)
        {
            var query = $"name = '{name}' and trashed = false";
            if (parentFolder != null) query += $" and '{parentFolder}' in parents";
            return query;
        }

        public Task<T> ExecuteRequestAsync<T>(Func<ClientServiceRequest<T>> request)
            => this.ExecuteRequestAsync(request, CancellationToken.None);

        public async Task<T> ExecuteRequestAsync<T>(Func<ClientServiceRequest<T>> request, CancellationToken token) =>
            await Policy.Handle<GoogleApiException>(e => e.HttpStatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(2, async (exception, span) =>
                {
                    this.logger.LogError(exception, $"Unauthorized exception during request to Google Drive");

                    var newAccessToken = await this.tenantApi.For(this.tenant)
                        .GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType.GoogleDrive,
                            this.refreshToken).ConfigureAwait(false);

                    this.CreateClient(newAccessToken);

                })
                .ExecuteAsync(async () => await request().ExecuteAsync(token));

        public void Dispose()
        {
            this.driveService?.Dispose();
            this.driveService = null;
        }
    }
}
