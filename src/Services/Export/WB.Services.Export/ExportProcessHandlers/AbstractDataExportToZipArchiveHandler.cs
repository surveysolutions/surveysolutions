using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class AbstractDataExportToZipArchiveHandler : BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        protected AbstractDataExportToZipArchiveHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.dataExportFileAccessor = dataExportFileAccessor;
        }

        protected override void DoExport(
            DataExportProcessDetails dataExportProcessDetails, 
            ExportSettings exportSettings, string archiveName,
            IProgress<int> exportProgress)
        {
            var tempArchivePath = this.fileSystemAccessor.CombinePath(this.exportTempDirectoryPath, this.fileSystemAccessor.GetFileName(archiveName));

            try
            {
                using (var archiveFile = fileSystemAccessor.OpenOrCreateFile(tempArchivePath, false))
                {
                    using (var archive = dataExportFileAccessor.CreateExportArchive(archiveFile, dataExportProcessDetails.ArchivePassword))
                    {
                        this.ExportDataIntoArchive(archive, exportSettings, exportProgress,
                            dataExportProcessDetails.CancellationToken);
                    }
                }

                fileSystemAccessor.DeleteFile(archiveName);
                fileSystemAccessor.MoveFile(tempArchivePath, archiveName);
                
                this.dataExportProcessesService.ChangeStatusType(dataExportProcessDetails.NaturalId, DataExportStatus.Compressing);
                exportProgress.Report(0);
                this.dataExportFileAccessor.PubishArchiveToExternalStorage(archiveName, exportProgress);

                dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                if (File.Exists(tempArchivePath)) File.Delete(tempArchivePath);
            }
        }

        protected abstract void ExportDataIntoArchive(IZipArchive archive, ExportSettings exportSettings, IProgress<int> exportProgress, CancellationToken cancellationToken);
    }
}
