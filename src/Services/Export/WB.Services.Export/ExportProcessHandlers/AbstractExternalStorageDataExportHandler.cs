using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal abstract class AbstractExternalStorageDataExportHandler : AbstractDataExportHandler,
        IExportProcessHandler<DataExportProcessDetails>
    {
        private readonly IBinaryDataSource binaryDataSource;

        protected AbstractExternalStorageDataExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IBinaryDataSource binaryDataSource) :
            base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;
        protected override bool CompressExportedData => false;
        
        protected override void ExportDataIntoDirectory(ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            //cancellationToken.ThrowIfCancellationRequested();
            
            using (this.GetClient(this.accessToken))
            {
                var applicationFolder = this.CreateApplicationFolderAsync().Result;

                string GetInterviewFolder(Guid interviewId) => $"{settings.ArchiveName})/{interviewId.FormatGuid()}";

                binaryDataSource.ForEachMultimediaAnswerAsync(settings, async data =>
                {
                    var interviewFolderPath = await this.CreateFolderAsync(applicationFolder, GetInterviewFolder(data.InterviewId));
                    await this.UploadFileAsync(interviewFolderPath, data.Content, data.Answer);

                }, cancellationToken).Wait();
            }
        }

        public override Task ExportDataAsync(DataExportProcessDetails process)
        {
            this.accessToken = process.AccessToken;
            return base.ExportDataAsync(process);
        }

        protected abstract IDisposable GetClient(string accessToken);
        private string accessToken;

        protected abstract Task<string> CreateApplicationFolderAsync();
        protected abstract Task<string> CreateFolderAsync(string applicationFolder, string folderName);

        protected abstract Task UploadFileAsync(string folder, byte[] fileContent, string fileName);
    }
}
