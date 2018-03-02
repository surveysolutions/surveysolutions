using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ExportToExternalStorage : DataExportProcessDetails
    {
        public ExternalStorageType StorageType { get; set; }
        public string AccessToken { get; set; }
        public ExportToExternalStorage(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle) : base(format, questionnaire, questionnaireTitle)
        {
        }
    }
}
