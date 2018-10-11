using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using File = Google.Apis.Drive.v3.Data.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class GoogleDriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public GoogleDriveBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IBinaryDataSource binaryDataSource,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, binaryDataSource)
        {
        }

        private DriveService driveService;
        private string GoogleDriveFolderMimeType = "application/vnd.google-apps.folder";

        protected override IDisposable GetClient(string accessToken)
        {
            var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
            {
                AccessToken = accessToken,
                ExpiresInSeconds = 3600,
                IssuedUtc = DateTime.UtcNow
            };

            var fakeflow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "fakeClientId",
                    ClientSecret = "fakeClientSecret"
                }
            });

            UserCredential credential = new UserCredential(fakeflow, "fakeUserId", token);
            var serviceInitializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            };

            this.driveService = new DriveService(serviceInitializer);
            return this.driveService;
        }

        protected override Task<string> CreateApplicationFolderAsync()
        {
            const string applicationFolderName = "Survey Solutions";
            return GetOrCreateFolderAsync(applicationFolderName);
        }

        protected override async Task<string> CreateFolderAsync(string applicationFolder, string folderName)
        {
            string[] folders = folderName.Split('/');

            string parentFolder = applicationFolder;

            foreach (var folder in folders)
            {
                parentFolder = await GetOrCreateFolderAsync(folder, parentFolder);
            }

            return parentFolder;
        }

        protected override async Task UploadFileAsync(string folder, byte[] fileContent, string fileName)
        {
            var file = await this.GetFileIdAsync(fileName, folder);
            if (file != null) return;

            var fileMetadata = new File
            {
                Name = fileName,
                Parents = new List<string> { folder }
            };

            var request = driveService.Files.Create(
                fileMetadata, new MemoryStream(fileContent), "application/octet-stream");

            await request.UploadAsync();
        }

        private Task<string> GetFileIdAsync(string filename, string parentFolderId = null)
        {
            var query = SearchQuery(filename, parentFolderId);
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<string> GetOrCreateFolderAsync(string folder, string parentId = null)
        {
            return await this.GetFolderIdAsync(folder, parentId) 
                ?? await this.ExecuteCreateFolderAsync(folder, parentId);
        }

        private async Task<string> ExecuteCreateFolderAsync(string folder, string parentId = null)
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
            var result = await request.ExecuteAsync();
            return result?.Id;
        }

        private Task<string> GetFolderIdAsync(string folderName, string parentFolderId = null)
        {
            var query = SearchQuery(folderName, parentFolderId);
            query += $" and mimeType = \'{GoogleDriveFolderMimeType}'";
            return SearchForFirstOccurenceAsync(query);
        }

        private async Task<string> SearchForFirstOccurenceAsync(string query)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id, name)";

            var files = (await listRequest.ExecuteAsync()).Files;

            return files.FirstOrDefault()?.Id;
        }

        private string SearchQuery(string name, string parentFolder)
        {
            var query = $"name = '{name}' and trashed = false";
            if (parentFolder != null) query += $" and '{parentFolder}' in parents";
            return query;
        }
    }
}
