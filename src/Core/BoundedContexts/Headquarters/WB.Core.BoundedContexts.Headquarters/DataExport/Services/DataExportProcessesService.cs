using System;
using System.Collections.Concurrent;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportProcessesService : IDataExportProcessesService
    {
        private readonly IAuditLog auditLog;
        private static readonly ConcurrentDictionary<string, DataExportProcessDetails> processes = 
            new ConcurrentDictionary<string, DataExportProcessDetails>();

        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;

        public DataExportProcessesService(IAuditLog auditLog, IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires)
        {
            this.auditLog = auditLog;
            this.questionnaires = questionnaires;
        }

        public DataExportProcessDetails GetAndStartOldestUnprocessedDataExport()
        {
            var exportProcess = processes.Values
                .Where(p => p.Status == DataExportStatus.Queued)
                .OrderBy(p => p.LastUpdateDate)
                .FirstOrDefault();

            if (exportProcess == null)
                return null;

            exportProcess.Status = DataExportStatus.Running;
            exportProcess.LastUpdateDate = DateTime.UtcNow;
            
            return exportProcess;
        }

        public void AddDataExport(DataExportProcessDetails details)
        {
            var questionnaireBrowseItem = this.questionnaires.GetById(details.Questionnaire.ToString());
            if (questionnaireBrowseItem == null)
                throw new ArgumentException($"Questionnaire {details.Questionnaire} wasn't found");
            
            this.EnqueueProcessIfNotYetInQueue(details);
        }

        private void EnqueueProcessIfNotYetInQueue(DataExportProcessDetails newProcess)
        {
            if (processes.GetOrNull(newProcess.NaturalId)?.IsQueuedOrRunning() ?? false)
                return;

            this.auditLog.ExportStared(newProcess.Name, newProcess.Format);
            processes[newProcess.NaturalId] = newProcess;
        }

        public DataExportProcessDetails[] GetRunningExportProcesses() 
            => processes.Values
            .Where(process => process.IsQueuedOrRunning())
            .OrderBy(p => p.BeginDate)
            .ToArray();

        public DataExportProcessDetails[] GetAllProcesses() => processes.Values.ToArray();

        public void FinishExportSuccessfully(string processId)
        {
            var dataExportProcess = processes.GetOrNull(processId);
            if (dataExportProcess == null) return;

            dataExportProcess.Status = DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;
        }

        public void FinishExportWithError(string processId, Exception e)
        {
            var dataExportProcess = processes.GetOrNull(processId);
            if (dataExportProcess == null) return;

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException(
                    $"Progress of data export process '{processId}' equals to '{progressInPercents}', but it can't be greater then 100 or less then 0");

            var dataExportProcess = processes.GetOrNull(processId);
            if (dataExportProcess == null) return;

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;
        }

        public void DeleteDataExport(string processId)
        {
            processes.GetOrNull(processId)?.Cancel();

            processes.TryRemove(processId, out _);
        }

        public void DeleteProcess(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var process = (IDataExportProcessDetails) new DataExportProcessDetails(exportFormat, questionnaire, null)
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            this.DeleteDataExport(process.NaturalId);
        }

        public void ChangeStatusType(string processId, DataExportStatus status)
        {
            var dataExportProcess = processes.GetOrNull(processId);
            if (dataExportProcess == null) return;

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.Status = status;
        }
    }
}
