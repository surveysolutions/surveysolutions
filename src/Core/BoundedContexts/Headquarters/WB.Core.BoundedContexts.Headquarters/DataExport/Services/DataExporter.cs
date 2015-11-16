using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
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

        private readonly Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcess>>> registeredExporters =
            new Dictionary<DataExportFormat, Dictionary<Type, Action<IDataExportProcess>>>();

        public DataExporter(IDataExportProcessesService dataExportProcessesService,
            ILogger logger)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            
            this.RegisterExportHandlerForFormat<ParaDataExportProcess, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<AllDataExportProcess, TabularFormatDataExportHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcess, TabularFormatDataExportHandler>(DataExportFormat.Tabular);

            this.RegisterExportHandlerForFormat<AllDataExportProcess, StataFormatExportHandler>(DataExportFormat.STATA);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcess, StataFormatExportHandler>(DataExportFormat.STATA);

            this.RegisterExportHandlerForFormat<AllDataExportProcess, SpssFormatExportHandler>(DataExportFormat.SPSS);
            this.RegisterExportHandlerForFormat<ApprovedDataExportProcess, SpssFormatExportHandler>(DataExportFormat.SPSS);

            this.RegisterExportHandlerForFormat<AllDataExportProcess, BinaryFormatDataExportHandler>(DataExportFormat.Binary);
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
                    IDataExportProcess dataExportProcess = this.dataExportProcessesService.GetAndStratOldestUnprocessedDataExport();

                    if (dataExportProcess == null)
                        return;
                    
                    try
                    {
                        HandleExportProcess(dataExportProcess);

                        this.dataExportProcessesService.FinishDataExport(dataExportProcess.DataExportProcessId);
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("data export process with id {0} finished with error", dataExportProcess.DataExportProcessId),
                            e);

                        this.dataExportProcessesService.FinishDataExportWithError(dataExportProcess.DataExportProcessId, e);
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void HandleExportProcess(IDataExportProcess dataExportProcess)
        {
            registeredExporters[dataExportProcess.DataExportFormat][dataExportProcess.GetType()](dataExportProcess);
        }

        private void RegisterExportHandlerForFormat<T, THandler>(DataExportFormat format)
            where T : class, IDataExportProcess
            where THandler : IExportProcessHandler<T>

        {
            if (!registeredExporters.ContainsKey(format))
                registeredExporters[format] = new Dictionary<Type, Action<IDataExportProcess>>();

            registeredExporters[format][typeof (T)] =
                (p) => { ServiceLocator.Current.GetInstance<THandler>().ExportData(p as T); };
        }
    }
}