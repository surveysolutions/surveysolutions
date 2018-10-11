using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    public class JobsStatusReporting : IJobsStatusReporting
    {
        private readonly Tuple<DataExportType, DataExportFormat>[] supportedDataExports = new[]
        {
            Tuple.Create(DataExportType.ParaData, DataExportFormat.Paradata),
            Tuple.Create(DataExportType.ParaData, DataExportFormat.Tabular),

            Tuple.Create(DataExportType.Data, DataExportFormat.Tabular),
            Tuple.Create(DataExportType.Data, DataExportFormat.STATA),
            Tuple.Create(DataExportType.Data, DataExportFormat.SPSS),
            Tuple.Create(DataExportType.Data, DataExportFormat.Binary),
        };

        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IQuestionnaireExportStructureFactory exportStructureFactory;
        private readonly IFilebasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;
        private readonly ILogger<JobsStatusReporting> logger;

        public JobsStatusReporting(IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireExportStructureFactory exportStructureFactory,
            IFilebasedExportedDataAccessor fileBasedExportedDataAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IExternalFileStorage externalFileStorage,
            IDataExportFileAccessor exportFileAccessor,
            ILogger<JobsStatusReporting> logger)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.exportStructureFactory = exportStructureFactory;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.logger = logger;
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var questionnaire = await
                this.exportStructureFactory.GetQuestionnaireExportStructureAsync(tenant, questionnaireIdentity);

            if (questionnaire == null)
                return null;

            var allProcesses = (await this.dataExportProcessesService.GetAllProcesses(tenant))
                .Select(CreateRunningDataExportProcessView).ToArray();

            var exports = new List<DataExportView>();

            foreach (var supportedDataExport in this.supportedDataExports)
            {
                var dataExportView = await this.CreateDataExportView(tenant, archiveFileName, supportedDataExport.Item1,
                    supportedDataExport.Item2, status,
                    fromDate, toDate, questionnaireIdentity, questionnaire, allProcesses);

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
                DataExportProcessName = dataExportProcessDetails.Name,
                Progress = dataExportProcessDetails.Status.ProgressInPercents,
                Format = dataExportProcessDetails.Format,
                ProcessStatus = dataExportProcessDetails.Status.Status,
                Type = dataExportProcessDetails.Format == DataExportFormat.Paradata
                    ? DataExportType.ParaData
                    : DataExportType.Data,
                QuestionnaireId = dataExportProcessDetails.Questionnaire.Id,
                InterviewStatus = dataExportProcessDetails.InterviewStatus,
                FromDate = dataExportProcessDetails.FromDate,
                ToDate = dataExportProcessDetails.ToDate
            };
        }

        private async Task<DataExportView> CreateDataExportView(TenantInfo tenant,
            string archiveFileName,
            DataExportType dataType,
            DataExportFormat dataFormat,
            InterviewStatus? interviewStatus,
            DateTime? fromDate,
            DateTime? toDate,
            QuestionnaireId questionnaireIdentity,
            QuestionnaireExportStructure questionnaire,
            RunningDataExportProcessView[] allProcesses)
        {
            DataExportView dataExportView = new DataExportView
            {
                DataExportFormat = dataFormat,
                DataExportType = dataType,
                StatusOfLatestExportProcess = GetStatusOfExportProcess(dataType, dataFormat, questionnaireIdentity, allProcesses)
            };

            if (dataFormat == DataExportFormat.Binary &&
                !questionnaire.HeaderToLevelMap.Values.SelectMany(l =>
                    l.HeaderItems.Values.OfType<ExportedQuestionHeaderItem>().Where(q =>
                        q.QuestionType == QuestionType.Multimedia || q.QuestionType == QuestionType.Audio)).Any())
            {
                dataExportView.CanRefreshBeRequested = false;
                dataExportView.HasAnyDataToBePrepared = false;
            }
            else
            {
                dataExportView.HasAnyDataToBePrepared = true;
                var process = allProcesses.FirstOrDefault(p =>
                    p.IsRunning &&
                    p.Format == dataFormat &&
                    p.Type == dataType &&
                    p.InterviewStatus == interviewStatus &&
                    p.FromDate == fromDate &&
                    p.ToDate == toDate &&
                    (p.QuestionnaireId == null || p.QuestionnaireId == questionnaireIdentity.Id));

                dataExportView.CanRefreshBeRequested = process == null;
                dataExportView.DataExportProcessId = process?.DataExportProcessId;
                dataExportView.ProgressInPercents = process?.Progress ?? 0;
            }

            string filePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                tenant,
                archiveFileName,
                dataFormat, interviewStatus, fromDate, toDate);

            if (this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, Path.GetFileName(filePath));
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
