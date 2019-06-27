using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using File = Google.Apis.Drive.v3.Data.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class GoogleDriveDataClient : IExternalDataClient
    {
        private DriveService driveService;
        private const string GoogleDriveFolderMimeType = "application/vnd.google-apps.folder";

        public IDisposable GetClient(string accessToken)
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
            return this.driveService;
        }

        public async Task<string> CreateApplicationFolderAsync()
        {
            const string applicationFolderName = "Survey Solutions";
            var folder = await GetOrCreateFolderAsync(applicationFolderName);
            return folder.Id;
        }

        public async Task<string> CreateFolderAsync(string applicationFolder, string folderName)
        {
            string[] folders = folderName.Split('/');

            string parentFolder = applicationFolder;

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
                await driveService.Files.Delete(file.Id).ExecuteAsync(cancellationToken);
            }

            await driveService.Files.Create(fileMetadata, fileStream, "application/octet-stream").UploadAsync(cancellationToken);
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            var storageInfoRequest = driveService.About.Get();
            storageInfoRequest.Fields = "storageQuota";

            var storageInfo = await storageInfoRequest.ExecuteAsync();
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

            var request = driveService.Files.Create(file);

            request.Fields = "id";
            return await request.ExecuteAsync();
        }

        private Task<File> GetFolderIdAsync(string folderName, string parentFolderId = null)
        {
            var query = SearchQuery(folderName, parentFolderId);
            query += $" and mimeType = \'{GoogleDriveFolderMimeType}'";
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<File> SearchForFirstOccurenceAsync(string query)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id, name)";

            var files = (await listRequest.ExecuteAsync()).Files;

            return files.FirstOrDefault();
        }

        private string SearchQuery(string name, string parentFolder)
        {
            var query = $"name = '{name}' and trashed = false";
            if (parentFolder != null) query += $" and '{parentFolder}' in parents";
            return query;
        }
    }
}
