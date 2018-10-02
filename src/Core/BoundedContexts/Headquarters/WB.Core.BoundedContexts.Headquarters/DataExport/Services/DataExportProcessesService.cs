using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportProcessesService : IDataExportProcessesService
    {
        private readonly IAuditLog auditLog;
        private readonly InterviewDataExportSettings settings;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IExportSettings exportSettings;

        private readonly ConcurrentDictionary<string, DataExportProcessDetails> processes = new ConcurrentDictionary<string, DataExportProcessDetails>();

        public DataExportProcessesService(IAuditLog auditLog,
            InterviewDataExportSettings settings, 
            IExportFileNameService exportFileNameService,
            IExportSettings exportSettings)
        {
            this.auditLog = auditLog;
            this.settings = settings;
            this.exportFileNameService = exportFileNameService;
            this.exportSettings = exportSettings;
        }

        public Task AddDataExportAsync(string baseUrl, string apiKey, DataExportProcessDetails details)
        {
            var api = RestService.For<IExportServiceApi>(settings.ExportServiceUrl);
            var archiveFileName = exportFileNameService.GetQuestionnaireTitleWithVersion(details.Questionnaire);

            return api.RequestUpdate(details.Questionnaire.ToString(), details.Format, details.InterviewStatus,
                details.FromDate, details.ToDate,
                archiveFileName, GetPasswordFromSettings(),
                details.AccessToken,
                details.StorageType,
                apiKey, baseUrl);
        }

        private string GetPasswordFromSettings()
        {
            return this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null;
        }

        private void EnqueueProcessIfNotYetInQueue(DataExportProcessDetails newProcess)
        {
            if (this.processes.GetOrNull(newProcess.NaturalId)?.IsQueuedOrRunning() ?? false)
                return;

            this.auditLog.ExportStared(newProcess.Name, newProcess.Format);
            this.processes[newProcess.NaturalId] = newProcess;
        }

        public DataExportProcessDetails[] GetRunningExportProcesses() 
            => this.processes.Values
            .Where(process => process.IsQueuedOrRunning())
            .OrderBy(p => p.BeginDate)
            .ToArray();

        public DataExportProcessDetails[] GetAllProcesses() => this.processes.Values.ToArray();

        public void DeleteDataExport(string processId)
        {
            this.processes.GetOrNull(processId)?.Cancel();

            this.processes.TryRemove(processId, out _);
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
    }
}
