using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportProcessesService: IDataExportProcessesService
    {
        private readonly ConcurrentDictionary<string, IDataExportDetails> dataExportProcessDtoStorage = new ConcurrentDictionary<string, IDataExportDetails>();
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires;

        public DataExportProcessesService(IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public IDataExportDetails GetAndStartOldestUnprocessedDataExport()
        {
            var exportProcess = dataExportProcessDtoStorage.Values
                .Where(p => p.Status == DataExportStatus.Queued)
                .OrderBy(p => p.LastUpdateDate)
                .FirstOrDefault();

            if (exportProcess == null)
                return null;

            exportProcess.Status = DataExportStatus.Running;
            exportProcess.LastUpdateDate = DateTime.UtcNow;
            
            return exportProcess;
        }

        public string AddAllDataExport(Guid questionnaireId, long questionnaireVersion,
            DataExportFormat exportFormat)
        {
            var questionnaire = questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException($"Questionnaire {questionnaireId.FormatGuid()} with version {questionnaireVersion} wasn't found");

            var exportProcess = new AllDataExportDetails(
                $"(ver. {questionnaireVersion}) {questionnaire.Title}",
                exportFormat,
                new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.Questionnaire.QuestionnaireId == questionnaireId && p.Questionnaire.Version == questionnaireVersion);
            return exportProcess.ProcessId;
        }

        public string AddApprovedDataExport(Guid questionnaireId, long questionnaireVersion,
         DataExportFormat exportFormat)
        {
            var questionnaire = questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (questionnaire == null)
                throw new ArgumentException($"Questionnaire {questionnaireId.FormatGuid()} with version {questionnaireVersion} wasn't found");

            var exportProcess = new ApprovedDataExportDetails(
                $"(ver. {questionnaireVersion}) {questionnaire.Title}, Approved",
                exportFormat,
                new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.Questionnaire.QuestionnaireId == questionnaireId && p.Questionnaire.Version == questionnaireVersion);
            return exportProcess.ProcessId;
        }

        public string AddParaDataExport(DataExportFormat exportFormat)
        {
            var exportProcess = new ParaDataExportDetails(exportFormat);

            this.AddDataExportProcessIfPossible(exportProcess, (p) => true);
            return exportProcess.ProcessId;
        }

        private void AddDataExportProcessIfPossible<T>(T exportProcess, Func<T,bool> additionalQuery) where T: IDataExportDetails
        {
            var runningOrQueuedDataExportProcessesByTheQuestionnaire =
                dataExportProcessDtoStorage.Values.OfType<T>().FirstOrDefault(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running) &&
                        p.Format == exportProcess.Format && additionalQuery(p));

            if (runningOrQueuedDataExportProcessesByTheQuestionnaire != null)
            {
                throw new InvalidOperationException();
            }

            dataExportProcessDtoStorage[exportProcess.ProcessId] = exportProcess;
        }

        public IDataExportDetails GetDataExport(string processId)
        {
            return dataExportProcessDtoStorage[processId];
        }

        public IDataExportDetails[] GetRunningDataExports()
        {
            return
                dataExportProcessDtoStorage.Values.Where(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running))
                    .OrderBy(p => p.BeginDate)
                    .ToArray();
        }

        public void FinishDataExport(string processId)
        {
            var dataExportProcess = this.GetDataExport(processId);
            if(dataExportProcess== null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status=DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;
        }

        public void FinishDataExportWithError(string processId, Exception e)
        {
            var dataExportProcess = this.GetDataExport(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            var dataExportProcess = this.GetDataExport(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;
        }

        public void DeleteDataExport(string processId)
        {
            dataExportProcessDtoStorage.Remove(processId);
        }
    }
}