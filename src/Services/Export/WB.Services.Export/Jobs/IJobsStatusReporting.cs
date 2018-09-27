using System;
using System.IO;
using System.Linq;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Jobs
{
    public interface IJobsStatusReporting
    {
        DataExportStatusView GetDataExportStatusForQuestionnaire(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }

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

        public DataExportStatusView GetDataExportStatusForQuestionnaire(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var questionnaire =
                this.exportStructureFactory.GetQuestionnaireExportStructure(tenant, questionnaireIdentity);

            if (questionnaire == null)
                return null;

            var runningProcesses = this.dataExportProcessesService.GetRunningExportProcesses(tenant).Select(CreateRunningDataExportProcessView).ToArray();
            var allProcesses = this.dataExportProcessesService.GetAllProcesses(tenant).Select(CreateRunningDataExportProcessView).ToArray();
            
            var dataExports = this.supportedDataExports.Select(supportedDataExport =>
                    this.CreateDataExportView(tenant, archiveFileName, supportedDataExport.Item1, supportedDataExport.Item2, status,
                        fromDate, toDate, questionnaireIdentity, questionnaire, runningProcesses, allProcesses))
                .ToArray();

            return new DataExportStatusView(
                questionnaireId: questionnaireIdentity,
                dataExports: dataExports,
                runningDataExportProcesses: runningProcesses);
        }

        private static RunningDataExportProcessView CreateRunningDataExportProcessView(IDataExportProcessDetails dataExportProcessDetails)
        {
            var exportProcessDetails = (DataExportProcessDetails)dataExportProcessDetails;

            return new RunningDataExportProcessView
            {
                DataExportProcessId = dataExportProcessDetails.NaturalId,
                BeginDate = dataExportProcessDetails.BeginDate,
                LastUpdateDate = dataExportProcessDetails.LastUpdateDate,
                DataExportProcessName = dataExportProcessDetails.Name,
                Progress = dataExportProcessDetails.ProgressInPercents,
                Format = dataExportProcessDetails.Format,
                ProcessStatus = dataExportProcessDetails.Status,
                Type = exportProcessDetails.Format == DataExportFormat.Paradata ? DataExportType.ParaData : DataExportType.Data,
                QuestionnaireId = exportProcessDetails.Questionnaire,
                InterviewStatus = exportProcessDetails.InterviewStatus,
                FromDate = exportProcessDetails.FromDate,
                ToDate = exportProcessDetails.ToDate
            };
        }

        private DataExportView CreateDataExportView(TenantInfo tenant,
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
                StatusOfLatestExportProcess = GetStatusOfExportProcess(dataType, dataFormat, questionnaireIdentity, allProcesses)
            };

            if (dataFormat == DataExportFormat.Binary &&
                !questionnaire.HeaderToLevelMap.Values.SelectMany(l => l.HeaderItems.Values.OfType<ExportedQuestionHeaderItem>().Where(q => q.QuestionType == QuestionType.Multimedia || q.QuestionType == QuestionType.Audio)).Any())
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

            if (dataFormat == DataExportFormat.Binary && this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(Path.GetFileName(filePath));
                var metadata = this.externalFileStorage.GetObjectMetadata(externalStoragePath);

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
