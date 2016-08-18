using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IDataExportProcessesService dataExportProcessesService;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly Tuple<DataExportType, DataExportFormat>[] supportedDataExports = new[]
        {
            Tuple.Create(DataExportType.ParaData, DataExportFormat.Tabular),

            Tuple.Create(DataExportType.Data, DataExportFormat.Tabular),
            Tuple.Create(DataExportType.Data, DataExportFormat.STATA),
            Tuple.Create(DataExportType.Data, DataExportFormat.SPSS),
            Tuple.Create(DataExportType.Data, DataExportFormat.Binary),
        };

        public DataExportStatusReader(
            IDataExportProcessesService dataExportProcessesService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IParaDataAccessor paraDataAccessor, 
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public DataExportStatusView GetDataExportStatusForQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null)
        {
            var questionnaire =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireIdentity);

            if (questionnaire == null)
                return null;

            var runningProcesses = this.dataExportProcessesService.GetRunningExportProcesses().Select(CreateRunningDataExportProcessView).ToArray();
            var allProcesses = this.dataExportProcessesService.GetAllProcesses().Select(CreateRunningDataExportProcessView).ToArray();

            var dataExports =
                this.supportedDataExports.Select(
                    supportedDataExport =>
                        this.CreateDataExportView(supportedDataExport.Item1, supportedDataExport.Item2, status, questionnaireIdentity, questionnaire, runningProcesses, allProcesses)).ToArray();

            return new DataExportStatusView(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                dataExports: dataExports,
                runningDataExportProcesses: runningProcesses);
        }

        private static RunningDataExportProcessView CreateRunningDataExportProcessView(IDataExportProcessDetails dataExportProcessDetails)
        {
            var result = new RunningDataExportProcessView
            {
                DataExportProcessId = dataExportProcessDetails.NaturalId,
                BeginDate = dataExportProcessDetails.BeginDate,
                LastUpdateDate = dataExportProcessDetails.LastUpdateDate,
                DataExportProcessName = dataExportProcessDetails.Name,
                Progress = dataExportProcessDetails.ProgressInPercents,
                Format = dataExportProcessDetails.Format,
                ProcessStatus = dataExportProcessDetails.Status
            };

            if (dataExportProcessDetails is ParaDataExportProcessDetails)
            {
                result.Type = DataExportType.ParaData;
            }
            else if (dataExportProcessDetails is DataExportProcessDetails)
            {
                result.Type = DataExportType.Data;
                var exportProcessDetails = (DataExportProcessDetails) dataExportProcessDetails;
                result.QuestionnaireIdentity = exportProcessDetails.Questionnaire;
                result.InterviewStatus = exportProcessDetails.InterviewStatus;
            }
            return result;
        }

        private DataExportView CreateDataExportView(DataExportType dataType,
            DataExportFormat dataFormat,
            InterviewStatus? interviewStatus, 
            QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireExportStructure questionnaire,
            RunningDataExportProcessView[] runningProcess,
            RunningDataExportProcessView[] allProcesses)
        {
            DataExportView dataExportView = null;
            dataExportView = new DataExportView
            {
                DataExportFormat = dataFormat,
                DataExportType = dataType,
                CanRefreshBeRequested = CanRefreshBeRequested(dataType, dataFormat, interviewStatus, questionnaireIdentity, questionnaire, runningProcess),
                StatusOfLatestExportProcess = GetStatusOfExportProcess(dataType, dataFormat, questionnaireIdentity, allProcesses)
            };

            string path = string.Empty;
            switch (dataType)
            {
                case DataExportType.ParaData:
                    path = this.paraDataAccessor.GetPathToParaDataArchiveByQuestionnaire(
                        questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                    break;
                case DataExportType.Data:
                    path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(questionnaireIdentity, dataFormat, interviewStatus);
                    break;
            }
            SetDataExportLastUpdateTimeIfFilePresent(dataExportView, path);
            return dataExportView;
        }

        private static DataExportStatus GetStatusOfExportProcess(DataExportType dataType, DataExportFormat dataFormat,
            QuestionnaireIdentity questionnaireIdentity, RunningDataExportProcessView[] allProcesses)
            => allProcesses.FirstOrDefault(x =>
                x.QuestionnaireIdentity == null ||
                x.QuestionnaireIdentity.Equals(questionnaireIdentity) && x.Format == dataFormat &&
                x.Type == dataType)?.ProcessStatus ?? DataExportStatus.NotStarted;

        private bool CanRefreshBeRequested(DataExportType dataType, 
            DataExportFormat dataFormat, 
            InterviewStatus? interviewStatus, 
            QuestionnaireIdentity questionnaireIdentity, 
            QuestionnaireExportStructure questionnaire, 
            RunningDataExportProcessView[] runningProcess)
        {
            if (dataFormat == DataExportFormat.Binary)
            {
                var hasMultimediaQuestions =
                    questionnaire.HeaderToLevelMap.Values.SelectMany(
                        l => l.HeaderItems.Values.Where(q => q.QuestionType == QuestionType.Multimedia)).Any();

                if (!hasMultimediaQuestions)
                    return false;
            }
            return !runningProcess.Any(
                p =>
                    p.Format == dataFormat &&
                    p.Type == dataType &&
                    p.InterviewStatus == interviewStatus &&
                    (p.QuestionnaireIdentity == null || p.QuestionnaireIdentity.Equals(questionnaireIdentity)));
        }

        private void SetDataExportLastUpdateTimeIfFilePresent(DataExportView dataExportView, string filePath)
        {
            if (fileSystemAccessor.IsFileExists(filePath))
            {
                dataExportView.LastUpdateDate = this.fileSystemAccessor.GetModificationTime(filePath);
                dataExportView.HasDataToExport = true;
            }
        }
    }
}