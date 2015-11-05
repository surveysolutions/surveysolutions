using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportQueue: IDataExportQueue
    {
        private readonly ConcurrentDictionary<string, IQueuedProcess> dataExportProcessDtoStorage=new ConcurrentDictionary<string, IQueuedProcess>();

        protected readonly ILogger Logger;

        public DataExportQueue(ILogger logger)
        {
            this.Logger = logger;
        }

        public string DeQueueDataExportProcessId()
        {
            var exportProcess = dataExportProcessDtoStorage.Values.Where(p => p.Status == DataExportStatus.Queued)
                .OrderBy(p => p.LastUpdateDate)
                .FirstOrDefault();

            if (exportProcess == null)
                return null;

            exportProcess.Status = DataExportStatus.Running;
            exportProcess.LastUpdateDate = DateTime.UtcNow;

            dataExportProcessDtoStorage[exportProcess.DataExportProcessId] = exportProcess;
            return exportProcess.DataExportProcessId;
        }

        public string EnQueueDataExportProcess(Guid questionnaireId, long questionnaireVersion,
            DataExportFormat exportFormat)
        {
            var runningOrQueuedDataExportProcessesByTheQuestionnaire =
                dataExportProcessDtoStorage.Values.OfType<AllDataQueuedProcess>().FirstOrDefault(
                            p =>
                                p.QuestionnaireId == questionnaireId && p.QuestionnaireVersion == questionnaireVersion &&
                                (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running));

            if (runningOrQueuedDataExportProcessesByTheQuestionnaire != null)
            {
                if (runningOrQueuedDataExportProcessesByTheQuestionnaire.Status == DataExportStatus.Queued)
                {
                    StartBackgroundDataExport();
                    return runningOrQueuedDataExportProcessesByTheQuestionnaire.DataExportProcessId;
                }

                throw new InvalidOperationException();
            }

            string processId = Guid.NewGuid().FormatGuid();

            var exportProcess = new AllDataQueuedProcess()
            {
                BeginDate = DateTime.UtcNow,
                DataExportProcessId = processId,
                DataExportFormat = exportFormat,
                LastUpdateDate = DateTime.UtcNow,
                ProgressInPercents = 0,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Status = DataExportStatus.Queued
            };
            
            dataExportProcessDtoStorage[processId] = exportProcess;
            StartBackgroundDataExport();
            return processId;
        }

        public string EnQueueParaDataExportProcess(DataExportFormat exportFormat)
        {
            var runningOrQueuedDataExportProcessesByTheQuestionnaire =
                dataExportProcessDtoStorage.Values.OfType<ParaDataQueuedProcess>().FirstOrDefault(
                            p =>
                                (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running));

            if (runningOrQueuedDataExportProcessesByTheQuestionnaire != null)
            {
                if (runningOrQueuedDataExportProcessesByTheQuestionnaire.Status == DataExportStatus.Queued)
                {
                    StartBackgroundDataExport();
                    return runningOrQueuedDataExportProcessesByTheQuestionnaire.DataExportProcessId;
                }

                throw new InvalidOperationException();
            }

            string processId = Guid.NewGuid().FormatGuid();
            var exportProcess = new ParaDataQueuedProcess()
            {
                BeginDate = DateTime.UtcNow,
                DataExportProcessId = processId,
                DataExportFormat = exportFormat,
                LastUpdateDate = DateTime.UtcNow,
                ProgressInPercents = 0,
                Status = DataExportStatus.Queued
            };
            dataExportProcessDtoStorage[processId] = exportProcess;

            StartBackgroundDataExport();
            return processId;
        }

        public IQueuedProcess GetDataExportProcess(string processId)
        {
            return dataExportProcessDtoStorage[processId];
        }

        public IQueuedProcess[] GetRunningProcess()
        {
            return
                dataExportProcessDtoStorage.Values.Where(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running)).ToArray();
        }

        public void FinishDataExportProcess(string processId)
        {
            var dataExportProcess = GetDataExportProcess(processId);
            if(dataExportProcess== null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status=DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;
            dataExportProcessDtoStorage[dataExportProcess.DataExportProcessId] = dataExportProcess;
        }

        public void FinishDataExportProcessWithError(string processId, Exception e)
        {
            var dataExportProcess = GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;

            dataExportProcessDtoStorage[dataExportProcess.DataExportProcessId] = dataExportProcess;
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            var dataExportProcess = GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;

            dataExportProcessDtoStorage[dataExportProcess.DataExportProcessId] = dataExportProcess;
        }

        public void DeleteDataExportProcess(string processId)
        {
            dataExportProcessDtoStorage.Remove(processId);
        }

        private void StartBackgroundDataExport()
        {
            new Thread(
                () =>
                {
                    ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                    try
                    {
                        ServiceLocator.Current.GetInstance<IDataExporter>().StartDataExport();
                    }
                    catch (Exception exc)
                    {
                        Logger.Error("Start of data export error ", exc);
                    }
                    finally
                    {
                        ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                    }
                }).Start();
        }
    }
}