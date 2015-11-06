using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferenceViewFactory :
        IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel>
    {
        private readonly IDataExportQueue dataExportQueue;

        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportedDataReferenceViewFactory(
            IDataExportQueue dataExportQueue, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IParaDataAccessor paraDataAccessor, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.dataExportQueue = dataExportQueue;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            var runningProcesses = dataExportQueue.GetRunningProcess();

            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                CreateExportedDataReferencesView(DataExportType.ParaData,DataExportFormat.Tabular, input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.Tabular, input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.ApprovedData, DataExportFormat.Tabular, input.QuestionnaireId, input.QuestionnaireVersion),
                runningProcesses.Select(
                    p =>
                        new RunningDataExportProcessView(p.DataExportProcessId, p.BeginDate, p.LastUpdateDate, "test",
                            1, p.ProgressInPercents, CreateDataExportType(p), p.DataExportFormat))
                    .ToArray());
        }

        private DataExportType CreateDataExportType(IQueuedProcess exportProcess)
        {
            if (exportProcess is ParaDataQueuedProcess)
                return DataExportType.ParaData;
            if (exportProcess is AllDataQueuedProcess)
                return DataExportType.Data;
            return DataExportType.ApprovedData;
        }

        private ExportedDataReferencesView CreateExportedDataReferencesView(DataExportType dataType,
            DataExportFormat dataFormat, Guid questionnaireId, long questionnaireVersion)
        {
            ExportedDataReferencesView exportedDataReferencesView = null;

            exportedDataReferencesView = new ExportedDataReferencesView()
            {
                DataExportFormat = dataFormat,
                CanRefreshBeRequested = true//latestDataProcess.Status != DataExportStatus.Running
            };

            if (dataType == DataExportType.ParaData)
            {
                var path = this.paraDataAccessor.GetPathToParaDataByQuestionnaire(questionnaireId, questionnaireVersion);
                if (fileSystemAccessor.IsFileExists(path))
                {
                    exportedDataReferencesView.LastUpdateDate = new FileInfo(path).LastWriteTime;
                    exportedDataReferencesView.HasDataToExport = true;
                }
            }
            if (dataType == DataExportType.Data)
            {
                var path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedTabularData(questionnaireId,
                    questionnaireVersion);
                if (fileSystemAccessor.IsFileExists(path))
                {
                    exportedDataReferencesView.LastUpdateDate = new FileInfo(path).LastWriteTime;
                    exportedDataReferencesView.HasDataToExport = true;
                }
            }
            if (dataType == DataExportType.ApprovedData)
            {
                var path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedTabularData(questionnaireId,
                    questionnaireVersion);
                if (fileSystemAccessor.IsFileExists(path))
                {
                    exportedDataReferencesView.LastUpdateDate = new FileInfo(path).LastWriteTime;
                    exportedDataReferencesView.HasDataToExport = true;
                }
            }
            return exportedDataReferencesView;
        }
    }

}