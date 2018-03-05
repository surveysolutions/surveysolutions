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
            ITransactionManager transactionManager,
            IInterviewFactory interviewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportFileAccessor dataExportFileAccessor,
            IAudioFileStorage audioFileStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor, questionnaireStorage, transactionManager,
                interviewFactory, imageFileRepository, audioFileStorage, plainTransactionManagerProvider)
        {
        }

        private DriveService driveService;
        protected override IDisposable GetClient(string accessToken)
        {
            var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
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
            var serviceInitializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            };

            this.driveService = new DriveService(serviceInitializer);
            return this.driveService;
        }

        protected override string CreateApplicationFolder()
        {
            var applicationFolderName = "Survey Solutions";

            var applicationFolder = this.GetFileOrDirectory(applicationFolderName, false);
            if (applicationFolder == null)
            {
                var request = driveService.Files.Create(new File
                {
                    Name = applicationFolderName,
                    MimeType = "application/vnd.google-apps.folder"
                });
                request.Fields = "id";
                applicationFolder = request.Execute();
            }

            return applicationFolder.Id;
        }

        protected override string CreateFolder(string applicatioFolder, string folderName)
        {
            var interviewFolder = this.GetFileOrDirectory(folderName, false, applicatioFolder);
            if (interviewFolder == null)
            {
                var request = driveService.Files.Create(new File
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> {applicatioFolder}
                });
                request.Fields = "id";
                interviewFolder = request.Execute();
            }

            return interviewFolder.Id;
        }

        private File GetFileOrDirectory(string folderOrFileName, bool isFile, string parentFolder = null)
        {
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = $"name = '{folderOrFileName}' and " +
                            (isFile ? "" : "mimeType = 'application/vnd.google-apps.folder' and ") +
                            $"trashed = false " +
                            $"{(parentFolder == null ? "" : $"and '{parentFolder}' in parents")}";
            listRequest.Fields = "files(id, name)";

            return listRequest.Execute().Files.FirstOrDefault();
        }

        protected override void UploadFile(string folder, byte[] fileContent, string fileName)
        {
            var file = this.GetFileOrDirectory(fileName, true, folder);
            if (file != null) return;

            var fileMetadata = new File
            {
                Name = fileName,
                Parents = new List<string> {folder}
            };

            var request = driveService.Files.Create(
                fileMetadata, new MemoryStream(fileContent), "application/octet-stream");

            request.Upload();
        }
    }
}
