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
        private readonly IPlainStorageAccessor<ExportedDataReference> exportedDataReferenceStorage;

        public ExportedDataReferenceViewFactory(IPlainStorageAccessor<ExportedDataReference> exportedDataReferenceStorage)
        {
            this.exportedDataReferenceStorage = exportedDataReferenceStorage;
        }

        public ExportedDataReferencesViewModel Load(ExportedDataReferenceInputModel input)
        {
            var paradataReference =
                exportedDataReferenceStorage.Query(
                    _ =>
                        _.Where(
                            d =>
                                d.DataExportType == DataExportType.Paradata &&
                                d.QuestionnaireId == input.QuestionnaireId &&
                                d.QuestionnaireVersion == input.QuestionnaireVersion)
                            .OrderByDescending(x => x.CreationTime)
                            .FirstOrDefault());

            return new ExportedDataReferencesViewModel(input.QuestionnaireId, input.QuestionnaireVersion,
                paradataReference);
        }
    }
}