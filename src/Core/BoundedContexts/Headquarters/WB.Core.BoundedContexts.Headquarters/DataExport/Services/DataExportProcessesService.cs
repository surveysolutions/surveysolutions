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

        public string AddAllDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat)
        {
            var questionnaire = questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException($"Questionnaire {questionnaireId.FormatGuid()} with version {questionnaireVersion} wasn't found");

            var exportProcess = new AllDataExportProcessDetails(
                $"(ver. {questionnaireVersion}) {questionnaire.Title}",
                exportFormat,
                new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.Questionnaire.QuestionnaireId == questionnaireId && p.Questionnaire.Version == questionnaireVersion);
            return exportProcess.ProcessId;
        }

        public string AddApprovedDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat)
        {
            var questionnaire = questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException($"Questionnaire {questionnaireId.FormatGuid()} with version {questionnaireVersion} wasn't found");

            var exportProcess = new ApprovedDataExportProcessDetails(
                $"(ver. {questionnaireVersion}) {questionnaire.Title}, Approved",
                exportFormat,
                new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.Questionnaire.QuestionnaireId == questionnaireId && p.Questionnaire.Version == questionnaireVersion);
            return exportProcess.ProcessId;
        }

        public string AddParaDataExport(DataExportFormat exportFormat)
        {
            var exportProcess = new ParaDataExportProcessDetails(exportFormat);

            this.AddDataExportProcessIfPossible(exportProcess, (p) => true);
            return exportProcess.ProcessId;
        }

        private void AddDataExportProcessIfPossible<T>(T exportProcess, Func<T,bool> additionalQuery) where T: IDataExportProcessDetails
        {
            var runningOrQueuedDataExportProcessesByTheQuestionnaire =
                this.processes.Values.OfType<T>().FirstOrDefault(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running) &&
                        p.Format == exportProcess.Format && additionalQuery(p));

            if (runningOrQueuedDataExportProcessesByTheQuestionnaire != null)
            {
                throw new InvalidOperationException();
            }

            this.processes[exportProcess.ProcessId] = exportProcess;
        }

        private IDataExportProcessDetails GetDataExportProcess(string processId)
        {
            return this.processes.GetOrNull(processId);
        }

        public IDataExportProcessDetails[] GetRunningDataExports()
        {
            return
                this.processes.Values.Where(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running))
                    .OrderBy(p => p.BeginDate)
                    .ToArray();
        }

        public void FinishExportSuccessfully(string processId)
        {
            var dataExportProcess = this.GetDataExportProcess(processId);
            if(dataExportProcess== null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status=DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;
        }

        public void FinishExportWithError(string processId, Exception e)
        {
            var dataExportProcess = this.GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            var dataExportProcess = this.GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;
        }

        public void DeleteDataExport(string processId)
        {
            this.processes.Remove(processId);
        }
    }
}