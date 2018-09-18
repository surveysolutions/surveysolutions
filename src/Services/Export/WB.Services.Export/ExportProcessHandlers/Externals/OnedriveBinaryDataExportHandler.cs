using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Storage;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OnedriveBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        public OnedriveBinaryDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IImageFileStorage imageFileRepository,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
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

        private OneDriveClient oneDriveClient;
        protected override IDisposable GetClient(string accessToken)
        {
            oneDriveClient = new OneDriveClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("bearer", accessToken);

                        return Task.CompletedTask;
                    }));

            return null;
        }

        protected override string CreateApplicationFolder() => "Survey Solutions";

        protected override string CreateFolder(string applicationFolder, string folderName) => $"{applicationFolder}/{folderName}";

        protected override void UploadFile(string folder, byte[] fileContent, string fileName)
        {
            var request = oneDriveClient.Drive.Root.ItemWithPath($"{folder}/{fileName}").Content.Request();
            request.PutAsync<Item>(new MemoryStream(fileContent)).Wait();
        }
    }
}
