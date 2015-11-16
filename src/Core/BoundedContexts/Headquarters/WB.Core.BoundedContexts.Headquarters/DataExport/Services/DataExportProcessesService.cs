using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ConcurrentDictionary<string, IDataExportProcess> dataExportProcessDtoStorage=new ConcurrentDictionary<string, IDataExportProcess>();
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires;
        protected readonly ILogger Logger;

        public DataExportProcessesService(ILogger logger, IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires)
        {
            this.Logger = logger;
            this.questionnaires = questionnaires;
        }

        public IDataExportProcess GetAndStratOldestUnprocessedDataExport()
        {
            var exportProcess = dataExportProcessDtoStorage.Values.Where(p => p.Status == DataExportStatus.Queued)
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
            {
                throw new ArgumentException("questionnaire wasn't found");
            }

            string processId = Guid.NewGuid().FormatGuid();

            var exportProcess = new AllDataExportProcess()
            {
                BeginDate = DateTime.UtcNow,
                DataExportProcessId = processId,
                DataExportFormat = exportFormat,
                LastUpdateDate = DateTime.UtcNow,
                ProgressInPercents = 0,
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                DataExportProcessName = string.Format("(ver. {1}) {0}", questionnaire.Title, questionnaireVersion),
                Status = DataExportStatus.Queued
            };

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.QuestionnaireIdentity.QuestionnaireId == questionnaireId && p.QuestionnaireIdentity.Version == questionnaireVersion);
            return processId;
        }

        public string AddApprovedDataExport(Guid questionnaireId, long questionnaireVersion,
         DataExportFormat exportFormat)
        {
            var questionnaire = questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);
            if (questionnaire == null)
            {
                throw new ArgumentException("questionnaire wasn't found");
            }

            string processId = Guid.NewGuid().FormatGuid();

            var exportProcess = new ApprovedDataExportProcess()
            {
                BeginDate = DateTime.UtcNow,
                DataExportProcessId = processId,
                DataExportFormat = exportFormat,
                LastUpdateDate = DateTime.UtcNow,
                ProgressInPercents = 0,
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion),
                DataExportProcessName = string.Format("(ver. {1}) {0}, Approved", questionnaire.Title, questionnaireVersion),
                Status = DataExportStatus.Queued
            };

            this.AddDataExportProcessIfPossible(exportProcess, (p) => p.QuestionnaireIdentity.QuestionnaireId == questionnaireId && p.QuestionnaireIdentity.Version == questionnaireVersion);
            return processId;
        }

        public string AddParaDataExport(DataExportFormat exportFormat)
        {
            string processId = Guid.NewGuid().FormatGuid();
            var exportProcess = new ParaDataExportProcess()
            {
                BeginDate = DateTime.UtcNow,
                DataExportProcessId = processId,
                DataExportFormat = exportFormat,
                LastUpdateDate = DateTime.UtcNow,
                ProgressInPercents = 0,
                DataExportProcessName = "ParaData",
                Status = DataExportStatus.Queued
            };
            this.AddDataExportProcessIfPossible(exportProcess, (p) => true);
            return processId;
        }

        private void AddDataExportProcessIfPossible<T>(T exportProcess, Func<T,bool> additionalQuery) where T: IDataExportProcess
        {
            var runningOrQueuedDataExportProcessesByTheQuestionnaire =
                dataExportProcessDtoStorage.Values.OfType<T>().FirstOrDefault(
                    p =>
                        (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running) &&
                        p.DataExportFormat == exportProcess.DataExportFormat && additionalQuery(p));

            if (runningOrQueuedDataExportProcessesByTheQuestionnaire != null)
            {
                throw new InvalidOperationException();
            }

            dataExportProcessDtoStorage[exportProcess.DataExportProcessId] = exportProcess;
        }

        public IDataExportProcess GetDataExport(string processId)
        {
            return dataExportProcessDtoStorage[processId];
        }

        public IDataExportProcess[] GetRunningDataExports()
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