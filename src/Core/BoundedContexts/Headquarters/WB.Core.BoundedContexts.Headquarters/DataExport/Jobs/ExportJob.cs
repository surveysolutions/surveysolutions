using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs
{
    [DisallowConcurrentExecution]
    internal class ExportJob : IJob
    {
        private IDataExportProcessesService exportService => ServiceLocator.Current
            .GetInstance<IDataExportProcessesService>();

        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<ExportJob>();

        public void Execute(IJobExecutionContext context)
        {
            ThreadMarkerManager.MarkCurrentThreadAsIsolated();
            ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();

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
            finally
            {
                ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                ThreadMarkerManager.RemoveCurrentThreadFromNoTransactional();
            }
        }

        private AbstractExternalStorageDataExportHandler GetExternalStorageExportHandler(ExternalStorageType storageType)
        {
            switch (storageType)
            {
              
                default:
                    throw new NotSupportedException($"Export handler for '{Enum.GetName(typeof(ExternalStorageType), storageType)}' not found");
            }
        }
        private BaseAbstractDataExportHandler GetExportHandler(DataExportFormat format)        {
            switch (format)
            {
                    case DataExportFormat.Binary:
                        return ServiceLocator.Current.GetInstance<BinaryFormatDataExportHandler>();
                    case DataExportFormat.Paradata:
                        return ServiceLocator.Current.GetInstance<TabularFormatParaDataExportProcessHandler>();
                case DataExportFormat.Tabular:
                        return ServiceLocator.Current.GetInstance<TabularFormatDataExportHandler>();
                    case DataExportFormat.SPSS:
                        return ServiceLocator.Current.GetInstance<SpssFormatExportHandler>();
                    case DataExportFormat.STATA:
                        return ServiceLocator.Current.GetInstance<StataFormatExportHandler>();
                default:
                    throw new NotSupportedException($"Export handler for '{format}' not found");
            }
        }
    }
}
