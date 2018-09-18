using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.ExportProcessHandlers.Externals;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;

namespace WB.Services.Export.Jobs
{
    internal class ExportJob
    {
        private readonly IDataExportProcessesService exportService;
        private readonly Lazy<BinaryFormatDataExportHandler> binaryFormatDataExportHandler;
        //private readonly Lazy<TabularFormatParaDataExportProcessHandler> tabularFormatParaDataExportProcessHandler;
        private readonly Lazy<TabularFormatDataExportHandler> tabularFormatDataExportHandler;
        //private readonly Lazy<SpssFormatExportHandler> spssFormatExportHandler;
        //private readonly Lazy<StataFormatExportHandler> stataFormatExportHandler;
        private readonly Lazy<OnedriveBinaryDataExportHandler> onedriveBinaryDataExportHandler;
        private readonly Lazy<DropboxBinaryDataExportHandler> dropboxBinaryDataExportHandler;
        private readonly Lazy<GoogleDriveBinaryDataExportHandler> googleDriveBinaryDataExportHandler;

        private readonly ILogger<ExportJob> logger;

        public ExportJob(IDataExportProcessesService exportService,
            Lazy<BinaryFormatDataExportHandler> binaryFormatDataExportHandler,
            //Lazy<TabularFormatParaDataExportProcessHandler> tabularFormatParaDataExportProcessHandler,
            Lazy<TabularFormatDataExportHandler> tabularFormatDataExportHandler,
            //Lazy<SpssFormatExportHandler> spssFormatExportHandler,
            //Lazy<StataFormatExportHandler> stataFormatExportHandler,
            Lazy<OnedriveBinaryDataExportHandler> onedriveBinaryDataExportHandler,
            Lazy<DropboxBinaryDataExportHandler> dropboxBinaryDataExportHandler,
            Lazy<GoogleDriveBinaryDataExportHandler> googleDriveBinaryDataExportHandler,
            ILogger<ExportJob> logger)
        {
            this.exportService = exportService;
            this.binaryFormatDataExportHandler = binaryFormatDataExportHandler;
            //this.tabularFormatParaDataExportProcessHandler = tabularFormatParaDataExportProcessHandler;
            this.tabularFormatDataExportHandler = tabularFormatDataExportHandler;
            //this.spssFormatExportHandler = spssFormatExportHandler;
            //this.stataFormatExportHandler = stataFormatExportHandler;
            this.onedriveBinaryDataExportHandler = onedriveBinaryDataExportHandler;
            this.dropboxBinaryDataExportHandler = dropboxBinaryDataExportHandler;
            this.googleDriveBinaryDataExportHandler = googleDriveBinaryDataExportHandler;
            this.logger = logger;
        }

        public async Task Execute(DataExportProcessDetails pendingExportProcess, CancellationToken cancellationToken)
        {
            try
            {
                if (pendingExportProcess is ExportBinaryToExternalStorage exportToExternalStorageProcess)
                    this.GetExternalStorageExportHandler(exportToExternalStorageProcess.StorageType).ExportData(exportToExternalStorageProcess);
                else
                    this.GetExportHandler(pendingExportProcess.Format).ExportData(pendingExportProcess);

                this.exportService.FinishExportSuccessfully(pendingExportProcess.NaturalId);

            }
            catch (Exception e)
            {
                this.exportService.FinishExportWithError(pendingExportProcess.NaturalId, e);

                this.logger.LogError("Export job failed", e);
            }
        }

        private AbstractExternalStorageDataExportHandler GetExternalStorageExportHandler(ExternalStorageType storageType)
        {
            switch (storageType)
            {
                case ExternalStorageType.OneDrive:
                    return onedriveBinaryDataExportHandler.Value;
                case ExternalStorageType.Dropbox:
                    return dropboxBinaryDataExportHandler.Value;
                case ExternalStorageType.GoogleDrive:
                    return googleDriveBinaryDataExportHandler.Value;
                default:
                    throw new NotSupportedException($"Export handler for '{Enum.GetName(typeof(ExternalStorageType), storageType)}' not found");
            }
        }
        private BaseAbstractDataExportHandler GetExportHandler(DataExportFormat format)
        {
            switch (format)
            {
                case DataExportFormat.Binary:
                    return binaryFormatDataExportHandler.Value;
                //case DataExportFormat.Paradata:
                //    return tabularFormatParaDataExportProcessHandler.Value;
                case DataExportFormat.Tabular:
                    return tabularFormatDataExportHandler.Value;
                //case DataExportFormat.SPSS:
                //    return spssFormatExportHandler.Value;
                //case DataExportFormat.STATA:
                //    return stataFormatExportHandler.Value;
                default:
                    throw new NotSupportedException($"Export handler for '{format}' not found");
            }
        }
    }
}
