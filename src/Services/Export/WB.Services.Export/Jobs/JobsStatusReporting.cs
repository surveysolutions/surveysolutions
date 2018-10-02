using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

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

        public JobsStatusReporting(IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireExportStructureFactory exportStructureFactory,
            IFilebasedExportedDataAccessor fileBasedExportedDataAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IExternalFileStorage externalFileStorage,
            IDataExportFileAccessor exportFileAccessor)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.exportStructureFactory = exportStructureFactory;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
        }

        public async Task<DataExportArchive> DownloadArchive(TenantInfo tenant, string archiveName,
            DataExportFormat dataExportFormat, InterviewStatus? status,
            DateTime? from, DateTime? to)
        {
            string filePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                tenant,
                archiveName,
                dataExportFormat, status, from, to);

            if (this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, Path.GetFileName(filePath));
                var metadata = await this.externalFileStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata != null)
                {
                    return new DataExportArchive
                    {
                        Redirect = new Uri(this.externalFileStorage.GetDirectLink(externalStoragePath, TimeSpan.FromSeconds(10)))
                    };
                }
            }
            else if (this.fileSystemAccessor.IsFileExists(filePath))
            {
                return new DataExportArchive
                {
                    FileName = Path.GetFileName(filePath),
                    Data = new FileStream(filePath, FileMode.Open, FileAccess.Read)
                };
            }

            return null;
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaire(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var questionnaire =
                this.exportStructureFactory.GetQuestionnaireExportStructure(tenant, questionnaireIdentity);

            if (questionnaire == null)
                return null;

            var runningProcesses = this.dataExportProcessesService.GetRunningExportProcesses(tenant)
                .Select(CreateRunningDataExportProcessView).ToArray();
            var allProcesses = this.dataExportProcessesService.GetAllProcesses(tenant)
                .Select(CreateRunningDataExportProcessView).ToArray();

            var exports = new List<DataExportView>();

            foreach (var supportedDataExport in this.supportedDataExports)
            {
                var dataExportView = await this.CreateDataExportView(tenant, archiveFileName, supportedDataExport.Item1,
                    supportedDataExport.Item2, status,
                    fromDate, toDate, questionnaireIdentity, questionnaire, runningProcesses, allProcesses);

                exports.Add(dataExportView);
            }

            return new DataExportStatusView(
                questionnaireId: questionnaireIdentity.Id,
                dataExports: exports,
                runningDataExportProcesses: runningProcesses);
        }

        private static RunningDataExportProcessView CreateRunningDataExportProcessView(
            IDataExportProcessDetails dataExportProcessDetails)
        {
            var exportProcessDetails = (DataExportProcessDetails) dataExportProcessDetails;

            return new RunningDataExportProcessView
            {
                DataExportProcessId = dataExportProcessDetails.NaturalId,
                BeginDate = dataExportProcessDetails.BeginDate,
                LastUpdateDate = dataExportProcessDetails.LastUpdateDate,
                DataExportProcessName = dataExportProcessDetails.Name,
                Progress = dataExportProcessDetails.ProgressInPercents,
                Format = dataExportProcessDetails.Format,
                ProcessStatus = dataExportProcessDetails.Status,
                Type = exportProcessDetails.Format == DataExportFormat.Paradata
                    ? DataExportType.ParaData
                    : DataExportType.Data,
                QuestionnaireId = exportProcessDetails.Questionnaire.Id,
                InterviewStatus = exportProcessDetails.InterviewStatus,
                FromDate = exportProcessDetails.FromDate,
                ToDate = exportProcessDetails.ToDate
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
            RunningDataExportProcessView[] runningProcess,
            RunningDataExportProcessView[] allProcesses)
        {
            DataExportView dataExportView = null;
            dataExportView = new DataExportView
            {
                DataExportFormat = dataFormat,
                DataExportType = dataType,
                StatusOfLatestExportProcess =
                    GetStatusOfExportProcess(dataType, dataFormat, questionnaireIdentity, allProcesses)
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
                var process = runningProcess.FirstOrDefault(p =>
                    p.Format == dataFormat &&
                    p.Type == dataType &&
                    p.InterviewStatus == interviewStatus &&
                    p.FromDate == fromDate &&
                    p.ToDate == toDate &&
                    (p.QuestionnaireId == null || p.QuestionnaireId.Equals(questionnaireIdentity)));

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
            => allProcesses.FirstOrDefault(x =>
                   x.QuestionnaireId == null ||
                   x.QuestionnaireId.Equals(questionnaireIdentity) && x.Format == dataFormat &&
                   x.Type == dataType)?.ProcessStatus ?? DataExportStatus.NotStarted;
    }
}
