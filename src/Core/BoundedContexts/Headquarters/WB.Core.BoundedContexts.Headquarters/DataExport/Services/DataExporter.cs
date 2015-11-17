using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExporter : IDataExporter
    {
        private bool IsWorking = false; //please use singleton injection

        private readonly IDataExportProcessesService dataExportProcessesService;

        private readonly ILogger logger;

        private readonly Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcessDetails>>> registeredExporters =
            new Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcessDetails>>>();

        public DataExporter(IDataExportProcessesService dataExportProcessesService,
            ILogger logger)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            
            this.RegisterExportHandlerForFormat<ParaDataExportProcessDetails, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<AllDataExportProcessDetails, TabularFormatDataExportHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcessDetails, TabularFormatDataExportHandler>(DataExportFormat.Tabular);

            this.RegisterExportHandlerForFormat<AllDataExportProcessDetails, StataFormatExportHandler>(DataExportFormat.STATA);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcessDetails, StataFormatExportHandler>(DataExportFormat.STATA);

            this.RegisterExportHandlerForFormat<AllDataExportProcessDetails, SpssFormatExportHandler>(DataExportFormat.SPSS);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcessDetails, SpssFormatExportHandler>(DataExportFormat.SPSS);

            this.RegisterExportHandlerForFormat<AllDataExportProcessDetails, BinaryFormatDataExportHandler>(DataExportFormat.Binary);
        }

        public void RunPendingDataExport()
        {
            if (IsWorking)
                return;

            IsWorking = true;
            try
            {
                while (IsWorking)
                {
                    IDataExportProcessDetails dataExportProcessDetails = this.dataExportProcessesService.GetAndStartOldestUnprocessedDataExport();

                    if (dataExportProcessDetails == null)
                        return;
                    
                    try
                    {
                        HandleExportProcess(dataExportProcessDetails);

                        this.dataExportProcessesService.FinishDataExport(dataExportProcessDetails.ProcessId);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Data export process with id {dataExportProcessDetails.ProcessId} finished with error", e);

                        this.dataExportProcessesService.FinishDataExportWithError(dataExportProcessDetails.ProcessId, e);
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void HandleExportProcess(IDataExportProcessDetails dataExportProcessDetails)
        {
            registeredExporters[dataExportProcessDetails.Format][dataExportProcessDetails.GetType()](dataExportProcessDetails);
        }

        private void RegisterExportHandlerForFormat<T, THandler>(DataExportFormat format)
            where T : class, IDataExportProcessDetails
            where THandler : IExportProcessHandler<T>

        {
            if (!registeredExporters.ContainsKey(format))
                registeredExporters[format] = new Dictionary<Type, Action<IDataExportProcessDetails>>();

            registeredExporters[format][typeof (T)] =
                (p) => { ServiceLocator.Current.GetInstance<THandler>().ExportData(p as T); };
        }
    }
}