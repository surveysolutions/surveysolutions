using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class BinaryFormatDataExportHandler : AbstractDataExportToZipArchiveHandler
    {
        private readonly IBinaryDataSource binaryDataSource;
        
        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor, IBinaryDataSource binaryDataSource)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override async Task ExportDataIntoArchiveAsync(IZipArchive archive, ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            await binaryDataSource.ForEachInterviewMultimediaAsync(settings,
                binaryDataAction =>
                {
                    var path = binaryDataAction.InterviewId.FormatGuid();
                    if (binaryDataAction.Type == BinaryDataType.AudioAudit)
                        path = this.fileSystemAccessor.CombinePath(path, interviewDataExportSettings.Value.AudioAuditFolderName);
                    path = this.fileSystemAccessor.CombinePath(path, binaryDataAction.FileName);
                    archive.CreateEntry(path, binaryDataAction.Content);
                    return Task.CompletedTask;
                },
                progress, 
                cancellationToken);
        }
    }
}
