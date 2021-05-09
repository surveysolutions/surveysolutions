using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Jobs
{
    public class JobsStatusReporting : IJobsStatusReporting
    {
        private readonly (DataExportType exportType, DataExportFormat format)[] supportedDataExports =
        {
            (DataExportType.ParaData, DataExportFormat.Paradata),
            (DataExportType.ParaData, DataExportFormat.Tabular),

            (DataExportType.Data, DataExportFormat.Tabular),
            (DataExportType.Data, DataExportFormat.STATA),
            (DataExportType.Data, DataExportFormat.SPSS),
            (DataExportType.Data, DataExportFormat.Binary),
        };

        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExternalArtifactsStorage externalArtifactsStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;
        private readonly ITenantContext tenantContext;

        public JobsStatusReporting(IDataExportProcessesService dataExportProcessesService,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IExternalArtifactsStorage externalArtifactsStorage,
            IDataExportFileAccessor exportFileAccessor,
            ITenantContext tenantContext)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.externalArtifactsStorage = externalArtifactsStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.tenantContext = tenantContext;
        }

        private TenantInfo Tenant => this.tenantContext.Tenant;

        public async Task<DataExportProcessView?> GetDataExportStatusAsync(long processId)
        {
            DataExportProcessArgs? process = await this.dataExportProcessesService.GetProcessAsync(processId);
            if (process == null) return null;

            if(!Tenant.Id.Equals(process.ExportSettings.Tenant.Id)) throw new ArgumentException("Cannot found process #" + processId, nameof(processId));

            var dataExportProcessView = await ToDataExportProcessView(process);

            return dataExportProcessView;
        }

        public async Task<List<DataExportProcessView>> GetDataExportStatusesAsync(long[] processIds)
        {
            var processes = await this.dataExportProcessesService.GetProcessesAsync(processIds);

            var result = new List<DataExportProcessView>();

            foreach (var process in processes)
            {
                if (!Tenant.Id.Equals(process.ExportSettings.Tenant.Id))
                    continue;

                var dataExportProcessView = await ToDataExportProcessView(process);

                result.Add(dataExportProcessView);
            }

            return result;
        }

        public async Task<IEnumerable<DataExportProcessView>> GetDataExportStatusesAsync(
            DataExportFormat? exportType, InterviewStatus? interviewStatus, string? questionnaireIdentity,
            DataExportJobStatus? exportStatus, bool? hasFile, int? limit, int? offset)
        {
            var allProcesses = await this.dataExportProcessesService.GetAllProcessesAsync(false);
                
            var allViews = new List<DataExportProcessView>();
            foreach (var process in allProcesses)
            {
                var view = await ToDataExportProcessView(process);

                if (IsInFilter(view, exportType, interviewStatus, questionnaireIdentity, exportStatus, hasFile))
                    allViews.Add(view);
            }

            var filteredViews = allViews.AsEnumerable();
            if (offset.HasValue)
                filteredViews = filteredViews.Skip(offset.Value);
            if (limit.HasValue)
                filteredViews = filteredViews.Take(limit.Value);

            return filteredViews.OrderByDescending(x => x.Id);
        }

        private bool IsInFilter(DataExportProcessView process, DataExportFormat? exportType,
            InterviewStatus? interviewStatus, string? questionnaireIdentity, DataExportJobStatus? exportStatus,
            bool? hasFile)
        {
            var hasFormat = !exportType.HasValue || process.Format == exportType;
            var hasInterviewStatus = !interviewStatus.HasValue || process.InterviewStatus == interviewStatus;
            var hasJobStatus = !exportStatus.HasValue || process.JobStatus == exportStatus;
            var hasHasFile = !hasFile.HasValue || process.HasFile == hasFile;
            var hasQuestionnaire = string.IsNullOrEmpty(questionnaireIdentity) ||
                                   process.QuestionnaireId == questionnaireIdentity;

            return hasFormat && hasInterviewStatus && hasJobStatus && hasHasFile && hasQuestionnaire;
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            var allProcesses = (await this.dataExportProcessesService.GetAllProcessesAsync())
                .Select(AsDataProcessView).ToArray();

            var exports = new List<DataExportView>();
            
            foreach (var supportedDataExport in this.supportedDataExports)
            {
                var exportSettings = new ExportSettings
                (
                    tenant : Tenant,
                    questionnaireId : questionnaireIdentity,
                    exportFormat : supportedDataExport.format,
                    status : status,
                    fromDate : fromDate,
                    toDate : toDate,
                    translation: null,
                    includeMeta: null
                );
                var dataExportView = await this.CreateDataExportView(exportSettings,
                    supportedDataExport.exportType, allProcesses);
                
                exports.Add(dataExportView);
            }

            return new DataExportStatusView
            {
                DataExports = exports,
                RunningDataExportProcesses = allProcesses.Where(p => p.IsRunning)
                    .Select(x => new RunningDataExportProcessView
                    {
                        
                    })
                    .ToList(),
                QuestionnaireId = questionnaireIdentity.Id.FormatGuid()
            };
            // questionnaireId: questionnaireIdentity.Id,
            // dataExports: exports,
            // runningDataExportProcesses: allProcesses.Where(p => p.IsRunning).ToArray());
        }

        private async Task<DataExportView> CreateDataExportView(
            ExportSettings exportSettings,
            DataExportType dataType,
            DataExportProcessView[] allProcesses)
        {
            DataExportView dataExportView = new DataExportView
            {
                DataExportFormat = exportSettings.ExportFormat,
                DataExportType = dataType,
                StatusOfLatestExportProcess = GetStatusOfExportProcess(dataType,
                    exportSettings.ExportFormat, exportSettings.QuestionnaireId,
                    allProcesses),
                HasAnyDataToBePrepared = true
            };

            var process = allProcesses.FirstOrDefault(p =>
                p.Format == exportSettings.ExportFormat &&
                p.Type == dataType &&
                p.InterviewStatus == exportSettings.Status &&
                p.FromDate == exportSettings.FromDate &&
                p.ToDate == exportSettings.ToDate &&
                (p.QuestionnaireId == null || p.QuestionnaireId == exportSettings.QuestionnaireId.ToString()));

            dataExportView.CanRefreshBeRequested = process?.IsRunning != true;
            dataExportView.DataExportProcessId = process?.Id.ToString();
            dataExportView.ProgressInPercents = process?.Progress ?? 0;
            dataExportView.TimeEstimation = process?.TimeEstimation;

            var exportFileInfo = await GetExportFileInfo(exportSettings);

            dataExportView.LastUpdateDate = exportFileInfo.LastUpdateDate;
            dataExportView.FileSize = exportFileInfo.FileSize;
            dataExportView.HasDataToExport = exportFileInfo.HasFile;

            dataExportView.Error = process?.Error;

            return dataExportView;
        }

        private static DataExportStatus GetStatusOfExportProcess(DataExportType dataType, DataExportFormat dataFormat,
            QuestionnaireIdentity questionnaireIdentity, DataExportProcessView[] allProcesses)
        {
            var matchingProcess = allProcesses.FirstOrDefault(x =>
                (x.QuestionnaireId == null || x.QuestionnaireId == questionnaireIdentity.ToString())
                && x.Format == dataFormat
                && x.Type == dataType);

            return matchingProcess?.ProcessStatus ?? DataExportStatus.NotStarted;
        }

        private async Task<DataExportProcessView> ToDataExportProcessView(DataExportProcessArgs process)
        {
            var dataExportProcessView = AsDataProcessView(process);
            var questionnaireId = dataExportProcessView.QuestionnaireId;

            var exportSettings = new ExportSettings
            (
                jobId: process.ProcessId,
                tenant : Tenant,
                questionnaireId: new QuestionnaireIdentity(questionnaireId),
                exportFormat : dataExportProcessView.Format,
                status : dataExportProcessView.InterviewStatus,
                fromDate : dataExportProcessView.FromDate,
                toDate : dataExportProcessView.ToDate,
                translation : dataExportProcessView.TranslationId,
                includeMeta: dataExportProcessView.IncludeMeta
            );

            dataExportProcessView.HasFile = false;

            var exportFileInfo = await GetExportFileInfo(exportSettings);

            dataExportProcessView.DataFileLastUpdateDate = exportFileInfo.LastUpdateDate;
            dataExportProcessView.FileSize = exportFileInfo.FileSize;
            dataExportProcessView.HasFile = exportFileInfo.HasFile;
            dataExportProcessView.DataDestination = process.StorageType.HasValue
                ? process.StorageType.Value.ToString()
                : "File";
            return dataExportProcessView;
        }

        private static DataExportProcessView AsDataProcessView(DataExportProcessArgs dataExportProcessDetails)
        {
            var status = dataExportProcessDetails.Status ?? new DataExportProcessStatus();
            var error = status.Error;
            var settings = dataExportProcessDetails.ExportSettings 
                           ?? new ExportSettings(exportFormat:DataExportFormat.Tabular, 
                               questionnaireId: new QuestionnaireIdentity(Guid.Empty.ToString()), 
                               new TenantInfo("",""));

            return new DataExportProcessView
            {
                Id = dataExportProcessDetails.ProcessId,
                IsRunning = status.IsRunning,
                BeginDate = status.BeginDate ?? status.CreatedDate,
                EndDate = status.EndDate,
                JobStatus = status.JobStatus,
                LastUpdateDate = status.LastUpdateDate,
                Progress = status.ProgressInPercents,
                TimeEstimation = status.TimeEstimation,
                Format = settings.ExportFormat,
                ProcessStatus = status.Status,
                Type = settings.ExportFormat == DataExportFormat.Paradata
                    ? DataExportType.ParaData
                    : DataExportType.Data,
                QuestionnaireId = settings.QuestionnaireId.ToString(),
                InterviewStatus = settings.Status,
                TranslationId = settings.Translation,
                IncludeMeta = settings.IncludeMeta,
                FromDate = settings.FromDate,
                ToDate = settings.ToDate,
                Error = error == null
                    ? null
                    : new DataExportErrorView
                    {
                        Type = status.Error?.Type ?? DataExportError.Unknown,
                        Message = status.Error?.Message ?? ""
                    }
            };
        }

        private async Task<ExportFileInfoView> GetExportFileInfo(ExportSettings exportSettings)
        {
            var exportFileInfo = new ExportFileInfoView
            {
                HasFile = false,
                FileSize = 0,
                LastUpdateDate = DateTime.MinValue
            };

            string filePath = await this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedDataAsync(exportSettings);
            if (this.externalArtifactsStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(exportSettings.Tenant, Path.GetFileName(filePath));
                var metadata = await this.externalArtifactsStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata == null) 
                    return exportFileInfo;

                exportFileInfo.LastUpdateDate = metadata.LastModified;
                exportFileInfo.FileSize = FileSizeUtils.SizeInMegabytes(metadata.Size);
                exportFileInfo.HasFile = true;
            }
            else if (this.fileSystemAccessor.IsFileExists(filePath))
            {
                exportFileInfo.LastUpdateDate = this.fileSystemAccessor.GetModificationTime(filePath);
                exportFileInfo.FileSize = FileSizeUtils.SizeInMegabytes(this.fileSystemAccessor.GetFileSize(filePath));
                exportFileInfo.HasFile = true;
            }

            return exportFileInfo;
        }
    }
}
