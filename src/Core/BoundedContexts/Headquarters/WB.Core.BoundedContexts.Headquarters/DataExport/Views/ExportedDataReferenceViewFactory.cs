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
                    LastUpdateDate = latestParaDataProcess.LastUpdateDate,
                    ProgressInPercents = latestParaDataProcess.ProgressInPercents,
                    StatusOfLatestExportprocess = latestParaDataProcess.Status
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
                    exportedParaDataReferencesView.HasDataToExport = true;
                }
            }
            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                exportedParaDataReferencesView);
        }
    }
}