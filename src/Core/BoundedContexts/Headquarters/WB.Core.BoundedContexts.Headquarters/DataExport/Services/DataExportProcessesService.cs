using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportProcessesService : IDataExportProcessesService
    {
        private readonly ConcurrentDictionary<string, IDataExportProcessDetails> processes = new ConcurrentDictionary<string, IDataExportProcessDetails>();

        private IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

        public DataExportProcessesService()
        {
        }

        public IDataExportProcessDetails GetAndStartOldestUnprocessedDataExport()
        {
            var exportProcess = this.processes.Values
                .Where(p => p.Status == DataExportStatus.Queued)
                .OrderBy(p => p.LastUpdateDate)
                .FirstOrDefault();

            if (exportProcess == null)
                return null;

            exportProcess.Status = DataExportStatus.Running;
            exportProcess.LastUpdateDate = DateTime.UtcNow;
            
            return exportProcess;
        }

        public string AddDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat, InterviewStatus? status)
        {
            var questionnaireBrowseItem = this.questionnaires.GetById(questionnaire.ToString());
            if (questionnaireBrowseItem == null)
                throw new ArgumentException($"Questionnaire {questionnaire} wasn't found");

            var process = new DataExportProcessDetails(exportFormat, questionnaire, questionnaireBrowseItem.Title)
            {
                InterviewStatus = status
            };
            this.EnqueueProcessIfNotYetInQueue(process);

            return process.NaturalId;
        }

        public string AddParaDataExport(DataExportFormat exportFormat)
        {
            var process = new ParaDataExportProcessDetails(exportFormat);

            this.EnqueueProcessIfNotYetInQueue(process);

            return process.NaturalId;
        }

        private void EnqueueProcessIfNotYetInQueue(IDataExportProcessDetails newProcess)
        {
            if (this.processes.GetOrNull(newProcess.NaturalId)?.IsQueuedOrRunning() ?? false)
                return;

            this.processes[newProcess.NaturalId] = newProcess;
        }

        public IDataExportProcessDetails[] GetRunningExportProcesses()
        {
            return this.processes.Values
                .Where(process => process.IsQueuedOrRunning())
                .OrderBy(p => p.BeginDate)
                .ToArray();
        }

        public IDataExportProcessDetails[] GetAllProcesses() => this.processes.Values.ToArray();

        public void FinishExportSuccessfully(string processId)
        {
            var dataExportProcess = this.processes.GetOrNull(processId);

            ThrowIfProcessIsNullOrNotRunningNow(dataExportProcess, processId);

            dataExportProcess.Status=DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;
        }

        public void FinishExportWithError(string processId, Exception e)
        {
            var dataExportProcess = this.processes.GetOrNull(processId);

            ThrowIfProcessIsNullOrNotRunningNow(dataExportProcess, processId);

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException(
                    $"Progress of data export process '{processId}' equals to '{progressInPercents}', but it can't be greater then 100 or less then 0");

            var dataExportProcess = this.processes.GetOrNull(processId);

            ThrowIfProcessIsNullOrNotRunningNow(dataExportProcess, processId);

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;
        }

        public void DeleteDataExport(string processId)
        {
            this.processes.GetOrNull(processId)?.Cancel();

            this.processes.TryRemove(processId);
        }

        public void DeleteProcess(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat, DataExportType exportType)
        {
            var process = exportType == DataExportType.Data
                ? (IDataExportProcessDetails) new DataExportProcessDetails(exportFormat, questionnaire, null)
                : new ParaDataExportProcessDetails(exportFormat);

            this.DeleteDataExport(process.NaturalId);
        }

        private static void ThrowIfProcessIsNullOrNotRunningNow(IDataExportProcessDetails dataExportProcess, string processId)
        {
            if (dataExportProcess == null)
                throw new InvalidOperationException($"Process with id '{processId}' is absent");

            if (dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException(
                    $"Process '{dataExportProcess.Name}' should be in Running state, but it is in state {dataExportProcess.Status}");
        }
    }
}