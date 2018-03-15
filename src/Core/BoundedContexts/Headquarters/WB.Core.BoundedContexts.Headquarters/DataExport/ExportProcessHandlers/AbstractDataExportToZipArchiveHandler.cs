using System;
using System.IO;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportToZipArchiveHandler : BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        protected AbstractDataExportToZipArchiveHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.dataExportFileAccessor = dataExportFileAccessor;
        }

        protected override void DoExport(DataExportProcessDetails dataExportProcessDetails, ExportSettings exportSettings, string archiveName,
            IProgress<int> exportProgress)
        {
            var tempArchivePath = this.fileSystemAccessor.CombinePath(this.exportTempDirectoryPath, this.fileSystemAccessor.GetFileName(archiveName));

            try
            {
                using (var archiveFile = fileSystemAccessor.OpenOrCreateFile(tempArchivePath, false))
                {
                    using (var archive = dataExportFileAccessor.CreateExportArchive(archiveFile, 0))
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
