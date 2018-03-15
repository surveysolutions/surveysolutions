using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler :BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        
        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
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

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 0);
            this.dataExportProcessesService.ChangeStatusType(dataExportProcessDetails.NaturalId,
                DataExportStatus.Compressing);

            this.dataExportFileAccessor.RecreateExportArchive(this.exportTempDirectoryPath, archiveName,
                exportProgress);
        }
        
        protected abstract void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}