using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly Tuple<DataExportType, DataExportFormat>[] supportedDataExports = new[]
        {
            Tuple.Create(DataExportType.ParaData, DataExportFormat.Tabular),

            Tuple.Create(DataExportType.Data, DataExportFormat.Tabular),
            Tuple.Create(DataExportType.ApprovedData, DataExportFormat.Tabular),

            Tuple.Create(DataExportType.Data, DataExportFormat.STATA),
            Tuple.Create(DataExportType.ApprovedData, DataExportFormat.STATA),

            Tuple.Create(DataExportType.Data, DataExportFormat.SPSS),
            Tuple.Create(DataExportType.ApprovedData, DataExportFormat.SPSS),

            Tuple.Create(DataExportType.Data, DataExportFormat.Binary),
        };

        public DataExportStatusReader(
            IDataExportProcessesService dataExportProcessesService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IParaDataAccessor paraDataAccessor, 
            IFileSystemAccessor fileSystemAccessor, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireReader = questionnaireReader;
        }

        public DataExportStatusView GetDataExportStatusForQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireReader.AsVersioned()
                .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version);

            if (questionnaire == null)
                return null;

            var runningProcesses = this.dataExportProcessesService.GetRunningExportProcesses().Select(CreateRunningDataExportProcessView).ToArray();

            var dataExports =
                this.supportedDataExports.Select(
                    supportedDataExport =>
                        this.CreateDataExportView(supportedDataExport.Item1, supportedDataExport.Item2, questionnaireIdentity,
                            questionnaire, runningProcesses)).ToArray();

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
                Format = dataExportProcessDetails.Format
            };

            if (dataExportProcessDetails is ParaDataExportProcessDetails)
            {
                result.Type = DataExportType.ParaData;
            }
            else if (dataExportProcessDetails is AllDataExportProcessDetails)
            {
                result.Type = DataExportType.Data;
                result.QuestionnaireIdentity = ((AllDataExportProcessDetails) dataExportProcessDetails).Questionnaire;
            }
            else if (dataExportProcessDetails is ApprovedDataExportProcessDetails)
            {
                result.Type = DataExportType.ApprovedData;
                result.QuestionnaireIdentity = ((ApprovedDataExportProcessDetails) dataExportProcessDetails).Questionnaire;
            }
            return result;
        }

        private DataExportView CreateDataExportView(
            DataExportType dataType,
            DataExportFormat dataFormat,
            QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireExportStructure questionnaire,
            RunningDataExportProcessView[] runningProcess)
        {
            DataExportView dataExportView = null;
            dataExportView = new DataExportView()
            {
                DataExportFormat = dataFormat,
                DataExportType = dataType,
                CanRefreshBeRequested =
                    CanRefreshBeRequested(dataType, dataFormat, questionnaireIdentity, questionnaire, runningProcess)
            };

            string path = string.Empty;
            switch (dataType)
            {
                case DataExportType.ParaData:
                    path = this.paraDataAccessor.GetPathToParaDataByQuestionnaire(
                        questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                    break;
                case DataExportType.Data:
                    path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(questionnaireIdentity,
                        dataFormat);
                    break;
                case DataExportType.ApprovedData:
                    path =
                        this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(
                            questionnaireIdentity, dataFormat);
                    break;
            }
            SetDataExportLastUpdateTimeIfFilePresent(dataExportView, path);
            return dataExportView;
        }

        private bool CanRefreshBeRequested(
            DataExportType dataType,
            DataExportFormat dataFormat,
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
                    (p.QuestionnaireIdentity == null ||
                     p.QuestionnaireIdentity.Equals(questionnaireIdentity)));
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