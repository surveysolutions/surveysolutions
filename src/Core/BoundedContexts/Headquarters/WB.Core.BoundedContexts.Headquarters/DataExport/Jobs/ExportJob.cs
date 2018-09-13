using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    [DisallowConcurrentExecution]
    internal class ExportJob : IJob
    {
        private IServiceLocator serviceLocator;

        public ExportJob(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        private IDataExportProcessesService exportService =>
            serviceLocator.GetInstance<IDataExportProcessesService>();

        private ILogger logger => serviceLocator.GetInstance<ILoggerProvider>().GetFor<ExportJob>();

        public void Execute(IJobExecutionContext context)
        {
            var pendingExportProcess = this.exportService.GetAndStartOldestUnprocessedDataExport();
            if (pendingExportProcess == null) return;

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

               this.logger.Error("Export job failed", e);
            }
        }

        private AbstractExternalStorageDataExportHandler GetExternalStorageExportHandler(ExternalStorageType storageType)
        {
            switch (storageType)
            {
                case ExternalStorageType.OneDrive:
                    return serviceLocator.GetInstance<OnedriveBinaryDataExportHandler>();
                case ExternalStorageType.Dropbox:
                    return serviceLocator.GetInstance<DropboxBinaryDataExportHandler>();
                case ExternalStorageType.GoogleDrive:
                    return serviceLocator.GetInstance<GoogleDriveBinaryDataExportHandler>();
                default:
                    throw new NotSupportedException($"Export handler for '{Enum.GetName(typeof(ExternalStorageType), storageType)}' not found");
            }
        }
        private BaseAbstractDataExportHandler GetExportHandler(DataExportFormat format)        {
            switch (format)
            {
                    case DataExportFormat.Binary:
                        return serviceLocator.GetInstance<BinaryFormatDataExportHandler>();
                    case DataExportFormat.Paradata:
                        return serviceLocator.GetInstance<TabularFormatParaDataExportProcessHandler>();
                case DataExportFormat.Tabular:
                        return serviceLocator.GetInstance<TabularFormatDataExportHandler>();
                    case DataExportFormat.SPSS:
                        return serviceLocator.GetInstance<SpssFormatExportHandler>();
                    case DataExportFormat.STATA:
                        return serviceLocator.GetInstance<StataFormatExportHandler>();
                default:
                    throw new NotSupportedException($"Export handler for '{format}' not found");
            }
        }
    }
}
