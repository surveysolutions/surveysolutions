using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using File = Google.Apis.Drive.v3.Data.File;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class GoogleDriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public GoogleDriveBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IImageFileStorage imageFileRepository,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IInterviewFactory interviewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportFileAccessor dataExportFileAccessor,
            IAudioFileStorage audioFileStorage)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, questionnaireStorage,
                interviewFactory, imageFileRepository, audioFileStorage)
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

        protected override string CreateApplicationFolder()
        {
            const string applicationFolderName = "Survey Solutions";
            return GetOrCreateFolder(applicationFolderName);
        }

        protected override string CreateFolder(string applicationFolder, string folderName)
        {
            string[] folders = folderName.Split('/');

            string parentFolder = applicationFolder;

            foreach (var folder in folders)
            {
                parentFolder = GetOrCreateFolder(folder, parentFolder);
            }

            return parentFolder;
        }

        protected override void UploadFile(string folder, byte[] fileContent, string fileName)
        {
            var file = this.GetFileId(fileName, folder);
            if (file != null) return;

            var fileMetadata = new File
            {
                Name = fileName,
                Parents = new List<string> { folder }
            };

            var request = driveService.Files.Create(
                fileMetadata, new MemoryStream(fileContent), "application/octet-stream");

            request.Upload();
        }

        private string GetFileId(string filename, string parentFolderId = null)
        {
            var query = SearchQuery(filename, parentFolderId);
            return SearchForFirstOccurence(query);
        }

        private string GetOrCreateFolder(string folder, string parentId = null)
        {
            return this.GetFolderId(folder, parentId) ?? this.ExecuteCreateFolder(folder, parentId);
        }

        private string ExecuteCreateFolder(string folder, string parentId = null)
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
            return request.Execute()?.Id;
        }

        private string GetFolderId(string folderName, string parentFolderId = null)
        {
            var query = SearchQuery(folderName, parentFolderId);
            query += $" and mimeType = \'{GoogleDriveFolderMimeType}'";
            return SearchForFirstOccurence(query);
        }

        private string SearchForFirstOccurence(string query)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id, name)";

            var files = listRequest.Execute().Files;

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
