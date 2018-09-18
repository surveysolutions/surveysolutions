using System;
using System.IO;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Storage;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class DropboxBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        private DropboxClient client;

        public DropboxBinaryDataExportHandler(
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

        protected override IDisposable GetClient(string accessToken)
        {
            this.client = new DropboxClient(accessToken);
            return client;
        }

        protected override string CreateApplicationFolder() => string.Empty;

        protected override string CreateFolder(string applicationFolder, string folderName) => $"/{folderName}";

        protected override void UploadFile(string folder, byte[] fileContent, string fileName)
            => this.client.Files.UploadAsync(new CommitInfo($"{folder}/{fileName}"), new MemoryStream(fileContent));

        //.WaitAndUnwrapException();
    }
}
