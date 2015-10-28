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
        private readonly IPlainStorageAccessor<ExportedDataReferenceDto> exportedDataReferenceStorage;
        private readonly IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage;

        public ExportedDataReferenceViewFactory(IPlainStorageAccessor<ExportedDataReferenceDto> exportedDataReferenceStorage, IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage)
        {
            this.exportedDataReferenceStorage = exportedDataReferenceStorage;
            this.dataExportProcessDtoStorage = dataExportProcessDtoStorage;
        }

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            var paradataReference =
                exportedDataReferenceStorage.Query(
                    _ =>
                        _.Where(
                            d =>
                                d.DataExportType == DataExportType.ParaData &&
                                d.QuestionnaireId == input.QuestionnaireId &&
                                d.QuestionnaireVersion == input.QuestionnaireVersion &&
                                d.DataExportProcessId != null)
                            .OrderByDescending(x => x.CreationDate)
                            .FirstOrDefault());
            ExportedDataReferencesView exportedDataReferencesView = null;
            if (paradataReference != null)
            {
                exportedDataReferencesView = new ExportedDataReferencesView()
                {
                    DataExportFormat = paradataReference.DataExportFormat,
                    ExportedDataReferenceId = paradataReference.ExportedDataReferenceId,
                    LastUpdateDate = paradataReference.CreationDate
                };
                if (string.IsNullOrEmpty(paradataReference.ExportedDataPath))
                {
                    var paraDataExportProcess =
                        dataExportProcessDtoStorage.GetById(paradataReference.ExportedDataReferenceId);
                    if (paraDataExportProcess != null)
                    {
                        exportedDataReferencesView.ExportedDataReferenceId = paradataReference.ExportedDataReferenceId;
                        exportedDataReferencesView.LastUpdateDate = paraDataExportProcess.LastUpdateDate;
                        exportedDataReferencesView.ProgressInPercents = paraDataExportProcess.ProgressInPercents;
                    }
                }
                else
                {
                    exportedDataReferencesView.HasDataToExport = true;
                    exportedDataReferencesView.LastUpdateDate = paradataReference.FinishDate;
                }
            }
            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                exportedDataReferencesView);
        }
    }
}