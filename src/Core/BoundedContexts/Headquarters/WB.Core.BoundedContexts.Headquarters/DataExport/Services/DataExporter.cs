using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExporter : IDataExporter
    {
        private static readonly object ExportLockObject = new object();

        private readonly IDataExportProcessesService dataExportProcessesService;

        private readonly ILogger logger;

        private readonly Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcessDetails>>> exporters =
            new Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcessDetails>>>();

        private readonly IServiceLocator serviceLocator;

        public DataExporter(IDataExportProcessesService dataExportProcessesService, ILogger logger, IServiceLocator serviceLocator)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            this.serviceLocator = serviceLocator;

            this.RegisterExporter<ParaDataExportProcessDetails, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Paradata);
            this.RegisterExporter<ParaDataExportProcessDetails, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExporter<DataExportProcessDetails, TabularFormatDataExportHandler>(DataExportFormat.Tabular);

            this.RegisterExporter<DataExportProcessDetails, StataFormatExportHandler>(DataExportFormat.STATA);
            this.RegisterExporter<DataExportProcessDetails, SpssFormatExportHandler>(DataExportFormat.SPSS);

            this.RegisterExporter<DataExportProcessDetails, BinaryFormatDataExportHandler>(DataExportFormat.Binary);
        }

        public void RunPendingExport()
        {
            lock (ExportLockObject)
            {
                IDataExportProcessDetails pendingExportProcess = this.dataExportProcessesService.GetAndStartOldestUnprocessedDataExport();

                if (pendingExportProcess == null)
                    return;

                try
                {
                    Action<IDataExportProcessDetails> exporter = this.GetExporter(pendingExportProcess);

                    exporter.Invoke(pendingExportProcess);

                    this.dataExportProcessesService.FinishExportSuccessfully(pendingExportProcess.NaturalId);
                }
                catch (Exception e)
                {
                    this.logger.Error($"Data export process '{pendingExportProcess.Name}' finished with error", e);

                    this.dataExportProcessesService.FinishExportWithError(pendingExportProcess.NaturalId, e);
                }
            }
        }

        private Action<IDataExportProcessDetails> GetExporter(IDataExportProcessDetails process)
        {
            return this.exporters[process.Format][process.GetType()];
        }

        private void RegisterExporter<TProcess, THandler>(DataExportFormat format)
            where TProcess : class, IDataExportProcessDetails
            where THandler : IExportProcessHandler<TProcess>
        {
            this.exporters.GetOrAdd(format)[typeof(TProcess)] = process => this.serviceLocator.GetInstance<THandler>().ExportData(process as TProcess);
        }
    }
}