using System;
using System.Threading;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler :BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        
        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor) 
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.dataExportFileAccessor = dataExportFileAccessor;
        }

        protected override void DoExport(DataExportProcessDetails dataExportProcessDetails,
            ExportSettings exportSettings, string archiveName, IProgress<int> exportProgress)
        {
            this.ExportDataIntoDirectory(exportSettings, exportProgress, dataExportProcessDetails.CancellationToken);

            if (!this.CompressExportedData) return;

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 0);
            this.dataExportProcessesService.ChangeStatusType(dataExportProcessDetails.NaturalId,
                DataExportStatus.Compressing);

            this.dataExportFileAccessor.RecreateExportArchive(this.exportTempDirectoryPath, archiveName, 
                dataExportProcessDetails.ArchivePassword, exportProgress);
        }

        protected virtual bool CompressExportedData => true;

        protected abstract void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
