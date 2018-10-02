using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : BaseAbstractDataExportHandler
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

        protected override async Task DoExportAsync(DataExportProcessDetails processArgs,
            ExportSettings exportSettings, string archiveName, IProgress<int> exportProgress)
        {
            this.ExportDataIntoDirectory(exportSettings, exportProgress, processArgs.CancellationToken);

            if (!this.CompressExportedData) return;

            processArgs.CancellationToken.ThrowIfCancellationRequested();

            this.dataExportProcessesService.UpdateDataExportProgress(processArgs.Tenant, processArgs.NaturalId, 0);
            this.dataExportProcessesService.ChangeStatusType(
                processArgs.Tenant,
                processArgs.NaturalId,
                DataExportStatus.Compressing);

            this.dataExportFileAccessor.RecreateExportArchive(this.exportTempDirectoryPath, archiveName,
                processArgs.ArchivePassword, exportProgress);

            await this.dataExportFileAccessor.PublishArchiveToExternalStorageAsync(processArgs.Tenant, archiveName, exportProgress);
        }

        protected virtual bool CompressExportedData => true;

        protected abstract void ExportDataIntoDirectory(ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
