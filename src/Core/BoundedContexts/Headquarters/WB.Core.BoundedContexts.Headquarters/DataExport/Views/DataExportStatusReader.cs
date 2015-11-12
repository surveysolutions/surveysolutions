using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IDataExportProcessesService dataExportProcessesService;

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
            IFileSystemAccessor fileSystemAccessor)
        {
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public DataExportStatusView GetDataExportStatusForQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var runningProcesses = this.dataExportProcessesService.GetRunningProcess().Select(CreateRunningDataExportProcessView).ToArray();
            var dataExports =
                this.supportedDataExports.Select(
                    supportedDataExport =>
                        this.CreateDataExportView(supportedDataExport.Item1, supportedDataExport.Item2,
                            questionnaireIdentity, runningProcesses)).ToArray();

            return new DataExportStatusView(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                dataExports: dataExports,
                runningDataExportProcesses: runningProcesses);
        }

        private RunningDataExportProcessView CreateRunningDataExportProcessView(IDataExportProcess exportProcess)
        {
            var result = new RunningDataExportProcessView()
            {
                DataExportProcessId = exportProcess.DataExportProcessId,
                BeginDate = exportProcess.BeginDate,
                LastUpdateDate = exportProcess.LastUpdateDate,
                DataExportProcessName = exportProcess.DataExportProcessName,
                Progress = exportProcess.ProgressInPercents,
                Format = exportProcess.DataExportFormat
            };

            if (exportProcess is ParaDataExportProcess)
            {
                result.Type = DataExportType.ParaData;
            }
            else if (exportProcess is AllDataExportProcess)
            {
                result.Type = DataExportType.Data;
                result.QuestionnaireIdentity = ((AllDataExportProcess) exportProcess).QuestionnaireIdentity;
            }
            else if (exportProcess is ApprovedDataExportProcess)
            {
                result.Type = DataExportType.ApprovedData;
                result.QuestionnaireIdentity = ((ApprovedDataExportProcess) exportProcess).QuestionnaireIdentity;
            }
            return result;
        }

        private DataExportView CreateDataExportView(
            DataExportType dataType,
            DataExportFormat dataFormat,
            QuestionnaireIdentity questionnaireIdentity,
            RunningDataExportProcessView[] runningProcess)
        {
            DataExportView dataExportView = null;

            dataExportView = new DataExportView()
            {
                DataExportFormat = dataFormat,
                DataExportType = dataType,
                CanRefreshBeRequested =
                    !runningProcess.Any(
                        p =>
                            p.Format == dataFormat && 
                            p.Type == dataType && 
                            (!p.QuestionnaireIdentity.HasValue || p.QuestionnaireIdentity.Value.Equals(questionnaireIdentity)))
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