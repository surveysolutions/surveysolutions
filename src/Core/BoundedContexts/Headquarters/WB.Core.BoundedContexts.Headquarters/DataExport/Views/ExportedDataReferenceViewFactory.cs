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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferenceViewFactory :
        IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel>
    {
        private readonly IDataExportProcessesService dataExportProcessesService;

        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportedDataReferenceViewFactory(
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

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            var runningProcesses = this.dataExportProcessesService.GetRunningProcess();

            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                CreateExportedDataReferencesView(DataExportType.ParaData, DataExportFormat.Tabular,
                    input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.Tabular, input.QuestionnaireId,
                    input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.ApprovedData, DataExportFormat.Tabular,
                    input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.STATA, input.QuestionnaireId,
                    input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.ApprovedData, DataExportFormat.STATA,
                    input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.SPPS, input.QuestionnaireId,
                    input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.ApprovedData, DataExportFormat.SPPS,
                    input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.Binary, input.QuestionnaireId,
                    input.QuestionnaireVersion),
                runningProcesses.Select(
                    p =>
                        new RunningDataExportProcessView(p.DataExportProcessId, p.BeginDate, p.LastUpdateDate,
                            p.DataExportProcessName, p.ProgressInPercents, CreateDataExportType(p), p.DataExportFormat))
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
                DataExportType= dataType,
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
                var path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(new QuestionnaireIdentity(questionnaireId,
                    questionnaireVersion), dataFormat);
                if (fileSystemAccessor.IsFileExists(path))
                {
                    exportedDataReferencesView.LastUpdateDate = new FileInfo(path).LastWriteTime;
                    exportedDataReferencesView.HasDataToExport = true;
                }
            }
            if (dataType == DataExportType.ApprovedData)
            {
                var path = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(new QuestionnaireIdentity(questionnaireId,
                    questionnaireVersion), dataFormat);
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