using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    internal class MultimediaDataExportHandlerBase : IExportHandler
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IBinaryDataSource binaryDataSource;

        internal virtual MultimediaDataType MultimediaDataType { get; }

        public MultimediaDataExportHandlerBase(
            IFileSystemAccessor fileSystemAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IBinaryDataSource binaryDataSource, 
            IDataExportFileAccessor dataExportFileAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.binaryDataSource = binaryDataSource;
            this.dataExportFileAccessor = dataExportFileAccessor;
        }
        
        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        { 
            var tempArchivePath = this.fileSystemAccessor.CombinePath(
                state.ExportTempFolder, this.fileSystemAccessor.GetFileName(state.ArchiveFilePath));

            await using (var archiveFile = fileSystemAccessor.OpenOrCreateFile(tempArchivePath, false))
            {
                using var archive = dataExportFileAccessor.CreateExportArchive(archiveFile, state.ProcessArgs.ArchivePassword);

                await binaryDataSource.ForEachInterviewMultimediaAsync(state, MultimediaDataType,
                    binaryDataAction =>
                    {
                        var path = binaryDataAction.InterviewKey ?? binaryDataAction.InterviewId.FormatGuid();
                        if (binaryDataAction.Type == BinaryDataType.AudioAudit)
                        {
                            path = this.fileSystemAccessor.CombinePath(path,
                                interviewDataExportSettings.Value.AudioAuditFolderName);
                        }

                        path = this.fileSystemAccessor.CombinePath(path, binaryDataAction.FileName);
                        archive.CreateEntry(path, binaryDataAction.Content, binaryDataAction.ContentLength);
                        return Task.CompletedTask;
                    }, cancellationToken);
            }

            fileSystemAccessor.DeleteFile(state.ArchiveFilePath);
            fileSystemAccessor.MoveFile(tempArchivePath, state.ArchiveFilePath);

            // it's already compressed
            state.RequireCompression = false;
            
            // we should prevent publish to external storage binary archive
            // for binary data we publish file by file, as size of archive can be too big for any external storage
            state.RequirePublishToExternalStorage = false;
        }
    }
}
