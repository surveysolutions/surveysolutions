using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedDataReferenceViewFactory: IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel>
    {
        private readonly IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage;

        public ExportedDataReferenceViewFactory(IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage)
        {
            this.dataExportProcessDtoStorage = dataExportProcessDtoStorage;
        }

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            ExportedDataReferencesView exportedParaDataReferencesView = null;

            var latestParaDataProcess =
              dataExportProcessDtoStorage.Query(
                  _ =>
                      _.Where(
                          d =>
                              d.DataExportType == DataExportType.ParaData)
                          .OrderByDescending(x => x.LastUpdateDate)
                          .FirstOrDefault());

            if (latestParaDataProcess != null)
            {
                exportedParaDataReferencesView = new ExportedDataReferencesView()
                {
                    DataExportFormat = latestParaDataProcess.DataExportFormat,
                    ProgressInPercents = latestParaDataProcess.ProgressInPercents,
                    StatusOfLatestExportprocess = latestParaDataProcess.Status,
                    CanRefreshBeRequested =
                        latestParaDataProcess.Status != DataExportStatus.Running
                };

                var latestCompletedParaDataProcess =
                    dataExportProcessDtoStorage.Query(
                        _ =>
                            _.Where(
                                d =>
                                    d.DataExportType == DataExportType.ParaData && d.Status == DataExportStatus.Finished)
                                .OrderByDescending(x => x.LastUpdateDate)
                                .FirstOrDefault());
                if (latestCompletedParaDataProcess != null)
                {
                    exportedParaDataReferencesView.LastUpdateDate = latestCompletedParaDataProcess.LastUpdateDate;
                    exportedParaDataReferencesView.HasDataToExport = true;
                }
            }

            var runningProcesses=
                dataExportProcessDtoStorage.Query(
                    _ =>
                        _.Where(d => d.Status == DataExportStatus.Queued || d.Status == DataExportStatus.Running)
                            .ToArray());

            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                exportedParaDataReferencesView,
                runningProcesses.Select(
                    p =>
                        new RunningDataExportProcessView(p.DataExportProcessId, p.BeginDate, p.LastUpdateDate, "test",
                            p.QuestionnaireVersion, p.ProgressInPercents, p.DataExportType, p.DataExportFormat)).ToArray());
        }
    }
}