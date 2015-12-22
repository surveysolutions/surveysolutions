using System;
using System.Collections.Concurrent;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportProcessesService : IDataExportProcessesService
    {
        private readonly ConcurrentDictionary<string, IDataExportProcessDetails> processes = new ConcurrentDictionary<string, IDataExportProcessDetails>();
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires;

        public DataExportProcessesService(IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires)
        {
            this.questionnaires = questionnaires;
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

        public string AddAllDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat)
        {
            var questionnaireBrowseItem = questionnaires.AsVersioned().Get(questionnaire.QuestionnaireId.FormatGuid(), questionnaire.Version);
            if (questionnaireBrowseItem == null)
                throw new ArgumentException($"Questionnaire {questionnaire} wasn't found");

            var process = new AllDataExportProcessDetails(exportFormat, questionnaire, questionnaireBrowseItem.Title);

            this.EnqueueProcessIfNotYetInQueue(process);

            return process.NaturalId;
        }

        public string AddApprovedDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat)
        {
            var questionnaireBrowseItem = questionnaires.AsVersioned().Get(questionnaire.QuestionnaireId.FormatGuid(), questionnaire.Version);
            if (questionnaireBrowseItem == null)
                throw new ArgumentException($"Questionnaire {questionnaire} wasn't found");

            var process = new ApprovedDataExportProcessDetails(exportFormat, questionnaire, questionnaireBrowseItem.Title);

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

            this.processes.Remove(processId);
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