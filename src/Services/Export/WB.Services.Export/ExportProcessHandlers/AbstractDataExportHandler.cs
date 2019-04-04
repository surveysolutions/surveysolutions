using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    public abstract class AbstractDataExportHandler : BaseAbstractDataExportHandler
    {
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        public AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.dataExportFileAccessor = dataExportFileAccessor;
        }

        public override async Task DoExportAsync(DataExportProcessArgs processArgs,
            ExportSettings exportSettings, string archiveName, 
            ExportProgress exportProgress, CancellationToken cancellationToken)
        {
            await this.ExportDataIntoDirectory(exportSettings, exportProgress, cancellationToken);

            if (!this.CompressExportedData) return;

            cancellationToken.ThrowIfCancellationRequested();

            this.dataExportProcessesService.UpdateDataExportProgress(processArgs.ProcessId, 0);

            this.dataExportProcessesService.ChangeStatusType(processArgs.ProcessId, DataExportStatus.Compressing);

            this.dataExportFileAccessor.RecreateExportArchive(this.ExportTempDirectoryPath, archiveName,
                processArgs.ArchivePassword, exportProgress);

            await this.dataExportFileAccessor.PublishArchiveToExternalStorageAsync(exportSettings.Tenant, archiveName, exportProgress);
        }

        protected virtual bool CompressExportedData => true;

        protected abstract Task ExportDataIntoDirectory(ExportSettings settings,
            ExportProgress progress,
            CancellationToken cancellationToken);
    }
}
