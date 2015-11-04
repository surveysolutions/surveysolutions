using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferenceViewFactory :
        IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel>
    {
        private readonly IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage;

        public ExportedDataReferenceViewFactory(IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage)
        {
            this.dataExportProcessDtoStorage = dataExportProcessDtoStorage;
        }

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            var runningProcesses =
                dataExportProcessDtoStorage.Query(
                    _ =>
                        _.Where(d => d.Status == DataExportStatus.Queued || d.Status == DataExportStatus.Running)
                            .ToArray());

            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                CreateExportedDataReferencesView(DataExportType.ParaData,DataExportFormat.TabularData, input.QuestionnaireId, input.QuestionnaireVersion),
                CreateExportedDataReferencesView(DataExportType.Data, DataExportFormat.TabularData, input.QuestionnaireId, input.QuestionnaireVersion),
                runningProcesses.Select(
                    p =>
                        new RunningDataExportProcessView(p.DataExportProcessId, p.BeginDate, p.LastUpdateDate, "test",
                            p.QuestionnaireVersion, p.ProgressInPercents, p.DataExportType, p.DataExportFormat))
                    .ToArray());
        }


        private ExportedDataReferencesView CreateExportedDataReferencesView(DataExportType dataType, DataExportFormat dataFormat, Guid? questionnaireId, long? questionnaireVersion)
        {
            ExportedDataReferencesView exportedDataReferencesView = null;

            var latestDataProcess =
                dataExportProcessDtoStorage.Query(
                    _ =>
                        _.Where(
                            d =>
                                d.DataExportType == dataType && d.DataExportFormat == dataFormat && d.QuestionnaireId==questionnaireId && d.QuestionnaireVersion== questionnaireVersion)
                            .OrderByDescending(x => x.LastUpdateDate)
                            .FirstOrDefault());

            if (latestDataProcess
                != null)
            {
                exportedDataReferencesView = new ExportedDataReferencesView()
                {
                    DataExportFormat = latestDataProcess.DataExportFormat,
                    ProgressInPercents = latestDataProcess.ProgressInPercents,
                    StatusOfLatestExportprocess = latestDataProcess.Status,
                    CanRefreshBeRequested =
                        latestDataProcess.Status != DataExportStatus.Running
                };

                var latestCompletedDataProcess =
                    dataExportProcessDtoStorage.Query(
                        _ =>
                            _.Where(
                                d =>
                                    d.DataExportType == dataType && d.DataExportFormat == dataFormat && d.Status == DataExportStatus.Finished && d.QuestionnaireId == questionnaireId && d.QuestionnaireVersion == questionnaireVersion)
                                .OrderByDescending(x => x.LastUpdateDate)
                                .FirstOrDefault());
                if (latestCompletedDataProcess != null)
                {
                    exportedDataReferencesView.LastUpdateDate = latestCompletedDataProcess.LastUpdateDate;
                    exportedDataReferencesView.HasDataToExport = true;
                }
            }

            return exportedDataReferencesView;
        }
    }

}