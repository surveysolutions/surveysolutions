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

        private readonly Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportDetails>>> registeredExporters =
            new Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportDetails>>>();

        public DataExporter(IDataExportProcessesService dataExportProcessesService,
            ILogger logger)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            
            this.RegisterExportHandlerForFormat<ParaDataExportDetails, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<AllDataExportDetails, TabularFormatDataExportHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<ApprovedDataExportDetails, TabularFormatDataExportHandler>(DataExportFormat.Tabular);

            this.RegisterExportHandlerForFormat<AllDataExportDetails, StataFormatExportHandler>(DataExportFormat.STATA);
            this.RegisterExportHandlerForFormat<ApprovedDataExportDetails, StataFormatExportHandler>(DataExportFormat.STATA);

            this.RegisterExportHandlerForFormat<AllDataExportDetails, SpssFormatExportHandler>(DataExportFormat.SPSS);
            this.RegisterExportHandlerForFormat<ApprovedDataExportDetails, SpssFormatExportHandler>(DataExportFormat.SPSS);

            this.RegisterExportHandlerForFormat<AllDataExportDetails, BinaryFormatDataExportHandler>(DataExportFormat.Binary);
        }

        public void StartDataExport()
        {
            if (IsWorking)
                return;

            IsWorking = true;
            try
            {
                while (IsWorking)
                {
                    IDataExportDetails dataExportDetails = this.dataExportProcessesService.GetAndStartOldestUnprocessedDataExport();

                    if (dataExportDetails == null)
                        return;
                    
                    try
                    {
                        HandleExportProcess(dataExportDetails);

                        this.dataExportProcessesService.FinishDataExport(dataExportDetails.ProcessId);
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("data export process with id {0} finished with error", dataExportDetails.ProcessId),
                            e);

                        this.dataExportProcessesService.FinishDataExportWithError(dataExportDetails.ProcessId, e);
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void HandleExportProcess(IDataExportDetails dataExportDetails)
        {
            registeredExporters[dataExportDetails.Format][dataExportDetails.GetType()](dataExportDetails);
        }

        private void RegisterExportHandlerForFormat<T, THandler>(DataExportFormat format)
            where T : class, IDataExportDetails
            where THandler : IExportProcessHandler<T>

        {
            if (!registeredExporters.ContainsKey(format))
                registeredExporters[format] = new Dictionary<Type, Action<IDataExportDetails>>();

            registeredExporters[format][typeof (T)] =
                (p) => { ServiceLocator.Current.GetInstance<THandler>().ExportData(p as T); };
        }
    }
}