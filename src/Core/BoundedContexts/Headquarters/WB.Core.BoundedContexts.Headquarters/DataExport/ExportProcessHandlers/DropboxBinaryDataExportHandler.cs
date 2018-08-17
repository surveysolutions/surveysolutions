using System;
using System.IO;
using Dropbox.Api;
using Dropbox.Api.Files;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class DropboxBinaryDataExportHandler : AbstractExternalStorageDataExportHandler
    {
        private DropboxClient client;

        public DropboxBinaryDataExportHandler(
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

        protected override IDisposable GetClient(string accessToken)
        {
            this.client = new DropboxClient(accessToken);
            return client;
        }

        protected override string CreateApplicationFolder() => string.Empty;

        protected override string CreateFolder(string applicationFolder, string folderName) => $"/{folderName}";

        protected override void UploadFile(string folder, byte[] fileContent, string fileName) 
            => this.client.Files.UploadAsync(new CommitInfo($"{folder}/{fileName}"), new MemoryStream(fileContent)).WaitAndUnwrapException();
    }
}
