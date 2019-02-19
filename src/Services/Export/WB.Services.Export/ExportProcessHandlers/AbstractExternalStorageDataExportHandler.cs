using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal abstract class AbstractExternalStorageDataExportHandler : AbstractDataExportHandler
    {
        private readonly IBinaryDataSource binaryDataSource;

        protected AbstractExternalStorageDataExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IBinaryDataSource binaryDataSource) :
            base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;
        protected override bool CompressExportedData => false;
        
        protected override async Task ExportDataIntoDirectory(ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            using (this.GetClient(this.accessToken))
            {
                var applicationFolder = await this.CreateApplicationFolderAsync();

                string GetInterviewFolder(Guid interviewId) => $"{settings.QuestionnaireId}/{interviewId.FormatGuid()}";
                string GetAudioAuditInterviewFolder(Guid interviewId) => $"{GetInterviewFolder(interviewId)}/{interviewDataExportSettings.Value.AudioAuditFolderName}";
                async Task<string> GetOrCreateFolderByType(BinaryDataType binaryDataType, Guid interviewId)
                {
                    switch (binaryDataType)
                    {
                        case BinaryDataType.Audio:
                        case BinaryDataType.Image:
                            return await this.CreateFolderAsync(applicationFolder, GetInterviewFolder(interviewId));
                        case BinaryDataType.AudioAudit:
                            return await this.CreateFolderAsync(applicationFolder, GetAudioAuditInterviewFolder(interviewId));
                        default:
                            throw new ArgumentException("Unknown binary type: " + binaryDataType);
                    }
                }

                await binaryDataSource.ForEachInterviewMultimediaAsync(settings, 
                    async binaryDataAction =>
                    {
                        var folderPath = await GetOrCreateFolderByType(binaryDataAction.Type, binaryDataAction.InterviewId);
                        await this.UploadFileAsync(folderPath, binaryDataAction.Content, binaryDataAction.FileName);
                    }, 
                    progress, cancellationToken);
            }
        }

        public override Task ExportDataAsync(DataExportProcessArgs process, CancellationToken cancellationToken)
        {
            this.accessToken = process.AccessToken;
            return base.ExportDataAsync(process, cancellationToken);
        }

        protected abstract IDisposable GetClient(string accessToken);
        private string accessToken;

        protected abstract Task<string> CreateApplicationFolderAsync();
        protected abstract Task<string> CreateFolderAsync(string applicationFolder, string folderName);

        protected abstract Task UploadFileAsync(string folder, byte[] fileContent, string fileName);
    }
}
