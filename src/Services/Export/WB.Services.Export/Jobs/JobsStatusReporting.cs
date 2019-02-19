using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

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
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;

        public JobsStatusReporting(IDataExportProcessesService dataExportProcessesService,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IExternalFileStorage externalFileStorage,
            IDataExportFileAccessor exportFileAccessor)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            var allProcesses = (await this.dataExportProcessesService.GetAllProcesses(tenant))
                .Select(CreateRunningDataExportProcessView).ToArray();

            var exports = new List<DataExportView>();

            foreach (var supportedDataExport in this.supportedDataExports)
            {
                var exportSettings = new ExportSettings
                {
                    Tenant = tenant,
                    QuestionnaireId = questionnaireIdentity,
                    ExportFormat = supportedDataExport.format,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate
                };
                var dataExportView = await this.CreateDataExportView(exportSettings, 
                    supportedDataExport.exportType, allProcesses);

                exports.Add(dataExportView);
            }

            return new DataExportStatusView(
                questionnaireId: questionnaireIdentity.Id,
                dataExports: exports,
                runningDataExportProcesses: allProcesses.Where(p => p.IsRunning).ToArray());
        }

        private static RunningDataExportProcessView CreateRunningDataExportProcessView(
            DataExportProcessArgs dataExportProcessDetails)
        {
            return new RunningDataExportProcessView
            {
                IsRunning = dataExportProcessDetails.Status.IsRunning,
                DataExportProcessId = dataExportProcessDetails.NaturalId,
                BeginDate = dataExportProcessDetails.Status.BeginDate ?? DateTime.MinValue,
                LastUpdateDate = dataExportProcessDetails.Status.LastUpdateDate,
                Progress = dataExportProcessDetails.Status.ProgressInPercents,
                TimeEstimation = dataExportProcessDetails.Status.TimeEstimation,
                Format = dataExportProcessDetails.ExportSettings.ExportFormat,
                ProcessStatus = dataExportProcessDetails.Status.Status,
                Type = dataExportProcessDetails.ExportSettings.ExportFormat == DataExportFormat.Paradata
                    ? DataExportType.ParaData
                    : DataExportType.Data,
                QuestionnaireId = dataExportProcessDetails.ExportSettings.QuestionnaireId.ToString(),
                InterviewStatus = dataExportProcessDetails.ExportSettings.Status,
                FromDate = dataExportProcessDetails.ExportSettings.FromDate,
                ToDate = dataExportProcessDetails.ExportSettings.ToDate
            };
        }

        private async Task<DataExportView> CreateDataExportView(
            ExportSettings exportSettings,
            DataExportType dataType,
            RunningDataExportProcessView[] allProcesses)
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
                p.IsRunning &&
                p.Format == exportSettings.ExportFormat &&
                p.Type == dataType &&
                p.InterviewStatus == exportSettings.Status &&
                p.FromDate == exportSettings.FromDate &&
                p.ToDate == exportSettings.ToDate &&
                (p.QuestionnaireId == null || p.QuestionnaireId == exportSettings.QuestionnaireId.ToString()));

            dataExportView.CanRefreshBeRequested = process == null;
            dataExportView.DataExportProcessId = process?.DataExportProcessId;
            dataExportView.ProgressInPercents = process?.Progress ?? 0;
            dataExportView.TimeEstimation = process?.TimeEstimation;

            string filePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(exportSettings);

            if (this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(exportSettings.Tenant, Path.GetFileName(filePath));
                var metadata = await this.externalFileStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata != null)
                {
                    dataExportView.LastUpdateDate = metadata.LastModified;
                    dataExportView.FileSize = FileSizeUtils.SizeInMegabytes(metadata.Size);
                    dataExportView.HasDataToExport = true;
                }
            }
            else if (this.fileSystemAccessor.IsFileExists(filePath))
            {
                dataExportView.LastUpdateDate = this.fileSystemAccessor.GetModificationTime(filePath);
                dataExportView.FileSize = FileSizeUtils.SizeInMegabytes(this.fileSystemAccessor.GetFileSize(filePath));
                dataExportView.HasDataToExport = true;
            }

            return dataExportView;
        }

        private static DataExportStatus GetStatusOfExportProcess(DataExportType dataType, DataExportFormat dataFormat,
            QuestionnaireId questionnaireIdentity, RunningDataExportProcessView[] allProcesses)
        {
            var matchingProcess = allProcesses.FirstOrDefault(x =>
                (x.QuestionnaireId == null || x.QuestionnaireId == questionnaireIdentity.ToString())
                && x.Format == dataFormat
                && x.Type == dataType);

            return matchingProcess?.ProcessStatus ?? DataExportStatus.NotStarted;
        }
    }
}
