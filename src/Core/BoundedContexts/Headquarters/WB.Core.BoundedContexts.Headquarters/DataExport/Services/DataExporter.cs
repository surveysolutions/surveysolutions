using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExporter : IDataExporter
    {
        private bool IsWorking = false; //please use singleton injection

        private readonly IDataExportQueue dataExportQueue;

        private readonly ILogger logger;

        private readonly Dictionary<DataExportFormat, Dictionary<Type, Action<IQueuedProcess>>> registeredExporters =
            new Dictionary<DataExportFormat, Dictionary<Type, Action<IQueuedProcess>>>();

        public DataExporter(IDataExportQueue dataExportQueue,
            ILogger logger)
        {
            this.dataExportQueue = dataExportQueue;
            this.logger = logger;
            
            this.RegisterExportHandlerForFormat<ParaDataQueuedProcess, TabularFormatParaDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<AllDataQueuedProcess, TabularFormatDataExportProcessHandler>(DataExportFormat.Tabular);
            this.RegisterExportHandlerForFormat<ApprovedDataQueuedProcess, TabularFormatDataExportProcessHandler>(DataExportFormat.Tabular);

            this.RegisterExportHandlerForFormat<AllDataQueuedProcess, StataFormatExportProcessHandler>(DataExportFormat.STATA);
            this.RegisterExportHandlerForFormat<ApprovedDataQueuedProcess, StataFormatExportProcessHandler>(DataExportFormat.STATA);

            this.RegisterExportHandlerForFormat<AllDataQueuedProcess, SpssFormatExportProcessHandler>(DataExportFormat.SPPS);
            this.RegisterExportHandlerForFormat<ApprovedDataQueuedProcess, SpssFormatExportProcessHandler>(DataExportFormat.SPPS);

            this.RegisterExportHandlerForFormat<AllDataQueuedProcess, BinaryFormatDataExportProcessHandler>(DataExportFormat.Binary);
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
                    IQueuedProcess dataExportProcess = this.dataExportQueue.DeQueueDataExportProcess();

                    if (dataExportProcess == null)
                        return;
                    
                    try
                    {
                        HandleExportProcess(dataExportProcess);

                        this.dataExportQueue.FinishDataExportProcess(dataExportProcess.DataExportProcessId);
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("data export process with id {0} finished with error", dataExportProcess.DataExportProcessId),
                            e);

                        this.dataExportQueue.FinishDataExportProcessWithError(dataExportProcess.DataExportProcessId, e);
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void HandleExportProcess(IQueuedProcess dataExportProcess)
        {
            registeredExporters[dataExportProcess.DataExportFormat][dataExportProcess.GetType()](dataExportProcess);
        }

        private void RegisterExportHandlerForFormat<T, THandler>(DataExportFormat format)
            where T : class, IQueuedProcess
            where THandler : IExportProcessHandler<T>

        {
            if (!registeredExporters.ContainsKey(format))
                registeredExporters[format] = new Dictionary<Type, Action<IQueuedProcess>>();

            registeredExporters[format][typeof (T)] =
                (p) => { ServiceLocator.Current.GetInstance<THandler>().ExportData(p as T); };
        }
    }
}